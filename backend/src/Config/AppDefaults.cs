namespace HackBackend.Config;

/// <summary>Стенд team-1 по умолчанию. Env, если задан, перекрывает.</summary>
public static class AppDefaults
{
    const string DefaultFrontendOrigin = "https://team-1.hack.kam-dev.ru";
    const string DefaultJwtSecret = "dev-secret-change-in-production-32ch";
    const string DefaultUploadsPath = "/app/data/uploads";
    const string DefaultVkGroupId = "238684561";
    const string DefaultVkMessagesIncludeGroupId = "1";

    public static string FrontendOrigin => Env("FRONTEND_ORIGIN", DefaultFrontendOrigin);
    public static string JwtSecret => Env("JWT_SECRET", "") is { Length: > 0 } secret
        ? secret
        : IsDevelopment
            ? DefaultJwtSecret
            : throw new InvalidOperationException("JWT_SECRET не задан (обязателен вне Development).");
    public static string UploadsPath => Env("UPLOADS_PATH", DefaultUploadsPath);
    public static string VkGroupAccessToken => Env("VK_GROUP_ACCESS_TOKEN", "");
    public static string VkGroupId => Env("VK_GROUP_ID", DefaultVkGroupId);
    public static string VkMessagesIncludeGroupId => Env("VK_MESSAGES_INCLUDE_GROUP_ID", DefaultVkMessagesIncludeGroupId);

    static bool IsDevelopment =>
        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);

    public static int JwtExpiresHours
    {
        get
        {
            var raw = Environment.GetEnvironmentVariable("JWT_EXPIRES_HOURS");
            return int.TryParse(raw, out var h) ? h : 24;
        }
    }

    public static string Env(string key, string fallback) =>
        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key))
            ? fallback
            : Environment.GetEnvironmentVariable(key)!.Trim();
}
