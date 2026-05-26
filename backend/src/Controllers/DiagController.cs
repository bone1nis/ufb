using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace HackBackend.Controllers;

[ApiController]
public class DiagController(IHttpClientFactory httpFactory) : ControllerBase
{
    // Два алиаса: /api/vk/diag и /api/vkdiag — на случай если прокси режет /api/vk/…
    [HttpGet("/api/vk/diag")]
    [HttpGet("/api/vkdiag")]
    public async Task<IActionResult> Diag([FromQuery] string? deep, CancellationToken ct)
    {
        var gid = Environment.GetEnvironmentVariable("VK_GROUP_ID");
        var tok = Environment.GetEnvironmentVariable("VK_GROUP_ACCESS_TOKEN");
        var wantDeep = string.Equals(deep, "1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(deep, "true", StringComparison.OrdinalIgnoreCase);

        string? longPollSettingsError = null;
        string? longPollSettingsJson  = null;

        if (wantDeep)
        {
            if (string.IsNullOrWhiteSpace(tok))
            {
                longPollSettingsError = "no_token";
            }
            else if (!TryParseGroupId(gid, out var gidNum))
            {
                longPollSettingsError = "bad_or_missing_vk_group_id";
            }
            else
            {
                try
                {
                    var url = $"https://api.vk.com/method/groups.getLongPollSettings?group_id={gidNum}&access_token={Uri.EscapeDataString(tok)}&v=5.199";
                    using var http = httpFactory.CreateClient();
                    http.Timeout = TimeSpan.FromSeconds(20);
                    using var resp = await http.GetAsync(url, ct);
                    var body = await resp.Content.ReadAsStringAsync(ct);
                    longPollSettingsJson = body.Length > 4000 ? body[..4000] + "…" : body;
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("error", out var err))
                        longPollSettingsError = err.ToString();
                }
                catch (Exception ex)
                {
                    longPollSettingsError = ex.GetType().Name + ": " + ex.Message;
                }
            }
        }

        return Ok(new
        {
            route                       = Request.Path.Value,
            vk_group_id                 = string.IsNullOrWhiteSpace(gid) ? null : gid,
            vk_token_configured         = !string.IsNullOrWhiteSpace(tok),
            vk_token_length             = tok?.Length ?? 0,
            aspnetcore_environment      = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            frontend_origin             = Environment.GetEnvironmentVariable("FRONTEND_ORIGIN"),
            long_poll_settings_requested = wantDeep,
            long_poll_settings_error    = longPollSettingsError,
            long_poll_settings_json     = longPollSettingsJson,
        });
    }

    static bool TryParseGroupId(string? raw, out long groupId)
    {
        groupId = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var g = raw.Trim();
        if (g.StartsWith("club", StringComparison.OrdinalIgnoreCase) && long.TryParse(g.AsSpan(4), out groupId))
        {
            groupId = Math.Abs(groupId);
            return groupId > 0;
        }
        if (!long.TryParse(g, out groupId)) return false;
        groupId = Math.Abs(groupId);
        return groupId > 0;
    }
}
