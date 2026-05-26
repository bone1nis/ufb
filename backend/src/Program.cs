using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using HackBackend.Auth;
using HackBackend.Config;
using HackBackend.Data;
using HackBackend.Options;
using HackBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

// Иначе JwtBearer переименует "role"/"sub" в длинные URI → UserRole() пустой → 403 на всех /api/*.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// ── JSON (snake_case + enum строкой — совпадает с ожиданиями Vue) ─────────────
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In          = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Bearer token",
        Name        = "Authorization",
        Type        = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme      = "bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});

// ── CORS ─────────────────────────────────────────────────────────────────────
var frontendOrigin = GetFrontendOrigin();
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(frontendOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(BuildConnectionString()));

// ── Auth (JWT cookie + Bearer) ────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        // В .NET 8 JsonWebTokenHandler по умолчанию переименовывает "role" → ClaimTypes.Role (URI).
        // MapInboundClaims = false оставляет клеймы как есть → RequireClaim("role", "...") работает.
        opt.MapInboundClaims = false;
        opt.TokenValidationParameters = JwtHelper.ValidationParameters();
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.TryGetValue("auth_token", out var token))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

// Роли хранятся как custom claim "role" (не ClaimTypes.Role), поэтому используем политики.
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("student",   p => p.RequireClaim("role", "student"));
    opts.AddPolicy("teacher",   p => p.RequireClaim("role", "teacher"));
    opts.AddPolicy("methodist", p => p.RequireClaim("role", "methodist"));
});

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddHttpClient<VkMessagesClient>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<VkLongPollWorker>();
builder.Services.AddHostedService<VkDeadlineReminderWorker>();

// ── Upload path ───────────────────────────────────────────────────────────────
var uploadsPath = AppDefaults.UploadsPath;
Directory.CreateDirectory(uploadsPath);
builder.Services.AddSingleton(new UploadSettings { Path = uploadsPath });

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseCors("Frontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

// Лёгкие health-эндпоинты — намеренно оставлены как Minimal API.
app.MapGet("/",        () => Results.Ok(new { service = "ufb-backend", status = "ok" }));
app.MapGet("/health",  () => Results.Ok(new { status = "ok", service = "backend" }));
app.MapGet("/api/ping",() => Results.Ok(new { message = "pong" }));

// ── Migrate + Seed ────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);
}

app.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("HackBackend")
    .LogInformation("VK-LP: перед app.Run() — после старта хоста должна появиться строка «VK-LP: фоновый цикл стартовал».");

app.Run();

// ── Config helpers ────────────────────────────────────────────────────────────
static string BuildConnectionString()
{
    var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "postgres";
    var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
    var db   = Environment.GetEnvironmentVariable("DB_NAME") ?? "hack_backend";
    var user = Environment.GetEnvironmentVariable("DB_USER") ?? "hack_user";
    var pass = Environment.GetEnvironmentVariable("DB_PASS") ?? "";
    return $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
}

static string GetFrontendOrigin() => AppDefaults.FrontendOrigin.TrimEnd('/');
