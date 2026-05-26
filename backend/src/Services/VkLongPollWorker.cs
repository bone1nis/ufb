using System.Text.Json;
using HackBackend.Config;
using HackBackend.Data;
using HackBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HackBackend.Services;

/// <summary>
/// Bots Long Poll (см. <see href="https://dev.vk.com/ru/api/bots-long-poll/getting-started"/>):
/// groups.getLongPollServer → a_check. Callback API не нужен.
/// </summary>
public sealed class VkLongPollWorker(
    IHttpClientFactory httpFactory,
    VkMessagesClient vk,
    IServiceScopeFactory scopeFactory,
    ILogger<VkLongPollWorker> logger) : BackgroundService
{
    const string ApiVersion = "5.199";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Information + префикс «VK-LP:» — как у «VK:» в VkMessagesClient, чтобы строки попадали в обычный grep / docker logs.
        logger.LogInformation("VK-LP: фоновый цикл стартовал");

        if (!vk.IsConfigured)
        {
            logger.LogInformation("VK-LP: не запущен: пустой VK_GROUP_ACCESS_TOKEN");
            return;
        }

        if (!TryParseGroupId(out var groupId))
        {
            logger.LogInformation("VK-LP: не запущен: не удалось разобрать VK_GROUP_ID");
            return;
        }

        // Именованный клиент мог не быть зарегистрирован — берём дефолтный из фабрики.
        var http = httpFactory.CreateClient();
        http.Timeout = TimeSpan.FromSeconds(90);

        string? server = null;
        string? key = null;
        var ts = "0";

        logger.LogInformation("VK-LP: ожидание событий для group_id={GroupId}, API v={V}", groupId, ApiVersion);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (server is null || key is null)
                {
                    var session = await FetchLongPollSessionAsync(http, groupId, stoppingToken);
                    server = session.Server;
                    key = session.Key;
                    ts = session.Ts;
                    if (server is null || key is null)
                    {
                        logger.LogInformation("VK-LP: сессия не получена, повтор через 10 с");
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                        continue;
                    }

                    logger.LogInformation("VK-LP: сессия Long Poll получена, ts={Ts}", ts);
                }

                var pollUrl = BuildPollUrl(server, key, ts);
                using var resp = await http.GetAsync(pollUrl, stoppingToken);
                var body = await resp.Content.ReadAsStringAsync(stoppingToken);

                if (!resp.IsSuccessStatusCode)
                    logger.LogWarning("VK-LP: a_check HTTP {Code}, тело: {Body}", (int)resp.StatusCode,
                        body.Length > 400 ? body[..400] + "…" : body);

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.TryGetProperty("failed", out var failedEl))
                {
                    var code = ReadFailedCode(failedEl);
                    // https://dev.vk.com/ru/api/bots-long-poll/getting-started — ошибки failed
                    if (code == 1)
                    {
                        logger.LogInformation("VK-LP: failed=1 (история/ts), обновляю ts, key сохраняю");
                        if (root.TryGetProperty("ts", out var newTs))
                            ts = ReadTsElement(newTs);
                        await Task.Delay(100, stoppingToken);
                        continue;
                    }

                    logger.LogWarning("VK-LP: failed={Code} — новая сессия getLongPollServer", code);
                    server = null;
                    key = null;
                    if (root.TryGetProperty("ts", out var tsOnFail))
                        ts = ReadTsElement(tsOnFail);
                    await Task.Delay(1500, stoppingToken);
                    continue;
                }

                if (root.TryGetProperty("ts", out var tsProp))
                    ts = ReadTsElement(tsProp);

                if (root.TryGetProperty("updates", out var updates))
                {
                    var n = updates.GetArrayLength();
                    if (n > 0)
                        logger.LogInformation("VK-LP: получено событий: {Count}", n);

                    foreach (var ev in updates.EnumerateArray())
                    {
                        try
                        {
                            await HandleUpdateAsync(ev, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "VK-LP: ошибка обработки одного update");
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "VK-LP: ошибка цикла");
                server = null;
                key = null;
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    static int ReadFailedCode(JsonElement failedEl) =>
        failedEl.ValueKind switch
        {
            JsonValueKind.Number => failedEl.GetInt32(),
            JsonValueKind.String => int.TryParse(failedEl.GetString(), out var c) ? c : -1,
            _ => -1,
        };

    static string ReadTsElement(JsonElement el) =>
        el.ValueKind switch
        {
            JsonValueKind.String => el.GetString()!,
            JsonValueKind.Number => el.GetInt64().ToString(),
            _ => el.GetRawText().Trim('"'),
        };

    static bool TryParseGroupId(out long groupId)
    {
        groupId = 0;
        var raw = AppDefaults.VkGroupId;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var g = raw.Trim();
        if (g.StartsWith("club", StringComparison.OrdinalIgnoreCase) && long.TryParse(g.AsSpan(4), out groupId))
        {
            groupId = Math.Abs(groupId);
            return true;
        }

        if (!long.TryParse(g, out groupId)) return false;
        groupId = Math.Abs(groupId);
        return groupId > 0;
    }

    async Task<(string? Server, string? Key, string Ts)> FetchLongPollSessionAsync(
        HttpClient http,
        long groupId,
        CancellationToken ct)
    {
        string? server = null;
        string? key = null;
        var ts = "0";

        var token = AppDefaults.VkGroupAccessToken;
        if (string.IsNullOrWhiteSpace(token))
            return (null, null, ts);

        var url =
            $"https://api.vk.com/method/groups.getLongPollServer?group_id={groupId}&access_token={Uri.EscapeDataString(token)}&v={ApiVersion}";
        using var resp = await http.GetAsync(url, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        logger.LogInformation("VK-LP: getLongPollServer HTTP {Code}", (int)resp.StatusCode);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.TryGetProperty("error", out var err))
        {
            logger.LogWarning("VK-LP: getLongPollServer API error: {Err}", err.ToString());
            return (null, null, ts);
        }

        if (!root.TryGetProperty("response", out var r))
        {
            logger.LogWarning("VK-LP: getLongPollServer: нет поля response, тело: {Body}",
                json.Length > 500 ? json[..500] + "…" : json);
            return (null, null, ts);
        }

        server = r.TryGetProperty("server", out var s) ? s.GetString() : null;
        key = r.TryGetProperty("key", out var k) ? k.GetString() : null;
        if (r.TryGetProperty("ts", out var t))
            ts = ReadTsElement(t);

        if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(key))
            logger.LogWarning("VK-LP: getLongPollServer: пустой server или key");

        return (server, key, ts);
    }

    static string BuildPollUrl(string server, string key, string ts)
    {
        var baseUrl = server.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? server.TrimEnd('/')
            : "https://" + server.TrimStart('/');
        return $"{baseUrl}?act=a_check&key={Uri.EscapeDataString(key)}&ts={Uri.EscapeDataString(ts)}&wait=25";
    }

    async Task HandleUpdateAsync(JsonElement ev, CancellationToken ct)
    {
        if (ev.ValueKind == JsonValueKind.Array)
        {
            logger.LogInformation(
                "VK-LP: событие в старом формате (массив) — пропуск. Включи Bots Long Poll и актуальные типы событий в ВК.");
            return;
        }

        if (ev.ValueKind != JsonValueKind.Object) return;
        if (!ev.TryGetProperty("type", out var typeEl)) return;
        var type = typeEl.GetString();
        if (type is not "message_new") return;
        if (!ev.TryGetProperty("object", out var obj)) return;

        JsonElement msg;
        if (obj.TryGetProperty("message", out var wrapped))
            msg = wrapped;
        else if (obj.TryGetProperty("from_id", out _))
            msg = obj;
        else
        {
            logger.LogInformation("VK-LP: message_new: неизвестный object, ключи: {Keys}",
                string.Join(", ", obj.EnumerateObject().Select(p => p.Name)));
            return;
        }

        if (IsOutgoingMessage(msg))
            return;

        if (!msg.TryGetProperty("from_id", out var fromEl))
            return;

        long fromLong = fromEl.ValueKind switch
        {
            JsonValueKind.Number => fromEl.GetInt64(),
            JsonValueKind.String => long.TryParse(fromEl.GetString(), out var x) ? x : 0,
            _ => 0,
        };
        if (fromLong <= 0) return;

        // Ответ нужно слать в тот же peer, что и входящее сообщение (чат сообщества, мультичат и т.д.).
        long peerForReply = fromLong;
        if (msg.TryGetProperty("peer_id", out var peerEl))
        {
            peerForReply = peerEl.ValueKind switch
            {
                JsonValueKind.Number => peerEl.GetInt64(),
                JsonValueKind.String => long.TryParse(peerEl.GetString(), out var px) ? px : fromLong,
                _ => fromLong,
            };
        }

        var text = msg.TryGetProperty("text", out var te) ? te.GetString()?.Trim() ?? "" : "";
        var head = NormalizeVkCommandHead(text);
        var sp = head.IndexOf(' ');
        if (sp > 0)
            head = head[..sp];

        logger.LogInformation("VK-LP: message_new from_id={FromId} peer_id={PeerId} head=\"{Head}\"", fromLong, peerForReply, head);

        switch (head)
        {
            case "/start":
            case "start":
            case "начать":
                await SendStartGuideAsync(peerForReply, fromLong, ct);
                return;
            case "/help":
            case "help":
            case "помощь":
                await SendBotHelpAsync(peerForReply, ct);
                return;
            case "/status":
            case "status":
            case "/статус":
            case "статус":
                try
                {
                    await SendAccountStatusAsync(peerForReply, fromLong, ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "VK-LP: ошибка /status");
                    await vk.TrySendUserMessageAsync(peerForReply.ToString(),
                        "Не удалось проверить привязку к сайту. Попробуй через минуту или напиши /start.", ct);
                }

                return;
            case "/deadlines":
            case "deadlines":
            case "/дедлайны":
            case "дедлайны":
            case "/дедлайн":
            case "дедлайн":
                try
                {
                    await SendStudentDeadlinesAsync(peerForReply, fromLong, ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "VK-LP: ошибка /deadlines");
                    await vk.TrySendUserMessageAsync(peerForReply.ToString(),
                        "Не удалось загрузить дедлайны. Попробуй через минуту.", ct);
                }

                return;
            default:
                if (!string.IsNullOrWhiteSpace(head))
                    logger.LogInformation("VK-LP: команда не распознана (покажи разработчику): head=\"{Head}\" len={Len}", head,
                        text.Length);
                return;
        }
    }

    /// <summary>
    /// Убирает невидимые символы и «полноширинный слэш» (часто с мобильных клавиатур) — иначе switch не совпадает с /status.
    /// </summary>
    static string NormalizeVkCommandHead(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        var s = raw.Trim()
            .Replace("\u200b", "", StringComparison.Ordinal)
            .Replace("\ufeff", "", StringComparison.Ordinal)
            .Replace("\u200c", "", StringComparison.Ordinal)
            .Replace("\u200d", "", StringComparison.Ordinal)
            .Replace("\uff0f", "/", StringComparison.Ordinal);
        var head = s.Split('@', 2)[0].Trim().ToLowerInvariant();
        var nl = head.IndexOf('\n');
        if (nl >= 0) head = head[..nl].Trim();
        return head;
    }

    async Task SendStartGuideAsync(long peerForReply, long fromVk, CancellationToken ct)
    {
        var origin = AppDefaults.FrontendOrigin.TrimEnd('/');
        var reply =
            "Привет! Это U.F.B. — Unified Feedback Board.\n\n" +
            "Чтобы получать уведомления о сдачах и оценках:\n" +
            $"1) Открой сайт: {origin}\n" +
            "2) Войди и вверху страницы в поле «ВК» укажи свой числовой id — у тебя он такой:\n" +
            $"👉 {fromVk}\n\n" +
            "Важно: напиши этому сообществу первым (или разреши ЛС от сообществ), иначе сайт не сможет прислать уведомление.\n\n" +
            "Команды в этом чате: /help — список.";

        logger.LogInformation("VK-LP: отвечаю messages.send → peer_id={Peer} (/start)", peerForReply);
        await vk.TrySendUserMessageAsync(peerForReply.ToString(), reply, ct);
    }

    async Task SendBotHelpAsync(long peerForReply, CancellationToken ct)
    {
        var origin = AppDefaults.FrontendOrigin.TrimEnd('/');
        var text =
            "Команды U.F.B. (бот сообщества):\n" +
            "• /start — как привязать ВК к сайту\n" +
            "• /status или /статус — привязан ли твой id к аккаунту на сайте\n" +
            "• /deadlines или /дедлайны — ближайшие дедлайны заданий (для студента)\n" +
            $"Сайт: {origin}";
        logger.LogInformation("VK-LP: отвечаю messages.send → peer_id={Peer} (/help)", peerForReply);
        await vk.TrySendUserMessageAsync(peerForReply.ToString(), text, ct);
    }

    async Task SendAccountStatusAsync(long peerForReply, long fromVk, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await FindUserByVkNumericAsync(db, fromVk, ct);
        string msg;
        if (user is null)
            msg =
                $"Аккаунт U.F.B. пока не привязан к id {fromVk}.\n" +
                "Зайди на сайт → авторизуйся → укажи этот id в поле «ВК» и сохрани.";
        else
            msg =
                "Аккаунт привязан.\n" +
                $"Email: {user.Email}\n" +
                $"Роль: {user.Role}\n" +
                $"Имя: {user.Name}";

        logger.LogInformation("VK-LP: отвечаю messages.send → peer_id={Peer} (/status)", peerForReply);
        await vk.TrySendUserMessageAsync(peerForReply.ToString(), msg, ct);
    }

    async Task SendStudentDeadlinesAsync(long peerForReply, long fromVk, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await FindUserByVkNumericAsync(db, fromVk, ct);

        if (user is null)
        {
            await vk.TrySendUserMessageAsync(peerForReply.ToString(),
                "Сначала привяжи ВК id к аккаунту на сайте (см. /start).", ct);
            return;
        }

        if (user.Role != UserRole.student)
        {
            await vk.TrySendUserMessageAsync(peerForReply.ToString(),
                $"Список дедлайнов в боте рассчитан на студента. У тебя роль: {user.Role}.", ct);
            return;
        }

        var submittedHw = await db.Submissions.AsNoTracking()
            .Where(s => s.StudentId == user.Id)
            .Select(s => s.HomeworkId)
            .ToListAsync(ct);
        var submittedSet = submittedHw.ToHashSet();

        var upcoming = await db.Homeworks.AsNoTracking()
            .Where(h => h.Deadline != null && h.Deadline > DateTime.UtcNow)
            .OrderBy(h => h.Deadline)
            .Take(15)
            .Select(h => new { h.Id, h.Title, h.Deadline, h.Project, h.Course })
            .ToListAsync(ct);

        if (upcoming.Count == 0)
        {
            await vk.TrySendUserMessageAsync(peerForReply.ToString(), "Нет заданий с дедлайном в будущем.", ct);
            return;
        }

        var lines = new List<string> { "Ближайшие дедлайны (UTC):" };
        foreach (var h in upcoming)
        {
            var st = submittedSet.Contains(h.Id) ? "сдано" : "не сдано";
            lines.Add($"• «{h.Title}» — {h.Deadline:dd.MM HH:mm} ({st}), курс {h.Course}, {h.Project}");
        }

        lines.Add("");
        lines.Add("Полный список заданий — на сайте в разделе студента.");

        var body = string.Join('\n', lines);
        if (body.Length > 3900)
            body = body[..3900] + "…";

        logger.LogInformation("VK-LP: отвечаю messages.send → peer_id={Peer} (/deadlines)", peerForReply);
        await vk.TrySendUserMessageAsync(peerForReply.ToString(), body, ct);
    }

    /// <summary>
    /// Сопоставление VK id с полем <see cref="User.VkUserId"/> без клиентских функций в LINQ — иначе EF Core не переводит в SQL и падает в runtime.
    /// </summary>
    static async Task<User?> FindUserByVkNumericAsync(AppDbContext db, long fromVk, CancellationToken ct)
    {
        var vkStr = fromVk.ToString();
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.VkUserId != null && u.VkUserId.Trim() == vkStr, ct);
        if (user is not null)
            return user;

        var withVk = await db.Users.AsNoTracking()
            .Where(u => u.VkUserId != null && u.VkUserId != "")
            .ToListAsync(ct);
        return withVk.FirstOrDefault(u => VkUserIdEquals(u.VkUserId, fromVk));
    }

    static bool VkUserIdEquals(string? stored, long vkNumeric)
    {
        if (string.IsNullOrWhiteSpace(stored)) return false;
        return long.TryParse(stored.Trim(), out var v) && v == vkNumeric;
    }

    static bool IsOutgoingMessage(JsonElement msg)
    {
        if (!msg.TryGetProperty("out", out var o))
            return false;
        return o.ValueKind switch
        {
            JsonValueKind.Number => o.GetInt32() != 0,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => false,
        };
    }
}
