using System.Text.Json;
using HackBackend.Config;

namespace HackBackend.Services;

/// <summary>
/// Исходящие сообщения ВК от имени сообщества (токен с правом messages).
/// Callback API для отправки не нужен — только токен и разрешение ЛС от пользователя.
/// </summary>
public sealed class VkMessagesClient(HttpClient http, ILogger<VkMessagesClient> logger)
{
    const string ApiVersion = "5.199";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(AppDefaults.VkGroupAccessToken);

    /// <param name="peerIdRaw">Целевой peer_id беседы (как во входящем message.peer_id), не только user id.</param>
    public async Task<bool> TrySendUserMessageAsync(string peerIdRaw, string text, CancellationToken ct = default)
    {
        var token = AppDefaults.VkGroupAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("VK: не отправлено — пустой VK_GROUP_ACCESS_TOKEN");
            return false;
        }

        if (!long.TryParse(peerIdRaw.Trim(), out var peerId))
        {
            logger.LogWarning("VK: неверный peer_id «{Peer}»", peerIdRaw);
            return false;
        }

        // По умолчанию не передаём group_id: токен сообщества уже привязан к группе; неверный VK_GROUP_ID ломает send.
        long? groupId = null;
        var includeGid = string.Equals(
            AppDefaults.VkMessagesIncludeGroupId,
            "1",
            StringComparison.OrdinalIgnoreCase);
        if (includeGid)
        {
            var groupIdRaw = AppDefaults.VkGroupId;
            if (!string.IsNullOrWhiteSpace(groupIdRaw))
            {
                var g = groupIdRaw.Trim();
                if (g.StartsWith("club", StringComparison.OrdinalIgnoreCase) && long.TryParse(g.AsSpan(4), out var cid))
                    groupId = cid;
                else if (long.TryParse(g, out var parsedId))
                    groupId = parsedId;
            }
        }

        var msg = text.Length > 4000 ? text[..4000] : text;
        var randomId = Random.Shared.Next(1, int.MaxValue);

        var form = new Dictionary<string, string>
        {
            ["access_token"] = token,
            ["v"] = ApiVersion,
            ["random_id"] = randomId.ToString(),
            ["peer_id"] = peerId.ToString(),
            ["message"] = msg,
        };
        if (groupId is { } gidValue)
            form["group_id"] = gidValue.ToString();

        logger.LogInformation(
            "VK: messages.send → peer_id={PeerId}, group_id={GroupId}, message_len={Len}",
            peerId,
            groupId?.ToString() ?? "(не передаётся)",
            msg.Length);

        using var content = new FormUrlEncodedContent(form);
        try
        {
            using var resp = await http.PostAsync("https://api.vk.com/method/messages.send", content, ct);
            var bodyText = await resp.Content.ReadAsStringAsync(ct);
            var preview = bodyText.Length > 1200 ? bodyText[..1200] + "…" : bodyText;
            logger.LogInformation("VK: HTTP {Status}, ответ API: {Body}", (int)resp.StatusCode, preview);

            if (!resp.IsSuccessStatusCode)
                return false;

            using var doc = JsonDocument.Parse(bodyText);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var err))
            {
                var code = err.TryGetProperty("error_code", out var c) ? c.GetInt32() : -1;
                var errMsg = err.TryGetProperty("error_msg", out var m) ? m.GetString() : err.ToString();
                logger.LogWarning("VK API error: code={Code}, msg={Msg}", code, errMsg);

                if (code is 901 or 7 or 962)
                    logger.LogWarning(
                        "VK: часто это «нет разрешения на ЛС». Напиши сообществу / боту в ЛС ВКонтакте первым " +
                        "(или включи сообщения от сообщества в настройках приватности). Callback API не нужен.");

                return false;
            }

            if (root.TryGetProperty("response", out var r))
            {
                logger.LogInformation("VK: сообщение принято, response={Response}", r.ToString());
                return true;
            }

            logger.LogWarning("VK: в JSON нет ни error, ни response");
            return false;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "VK: ответ не JSON");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "VK: сбой HTTP при messages.send");
            return false;
        }
    }
}
