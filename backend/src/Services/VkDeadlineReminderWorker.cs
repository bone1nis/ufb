using HackBackend.Data;
using HackBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HackBackend.Services;

/// <summary>
/// Периодически шлёт в ВК напоминания о дедлайнах студентам с привязанным VkUserId (без сдачи по заданию).
/// Дубли гасятся таблицей VkDeadlineReminderLogs.
/// </summary>
public sealed class VkDeadlineReminderWorker(
    IServiceScopeFactory scopeFactory,
    VkMessagesClient vk,
    ILogger<VkDeadlineReminderWorker> logger) : BackgroundService
{
    const int ReminderIntervalMinutes = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!vk.IsConfigured)
        {
            logger.LogInformation("VK-REM: не запущен — пустой VK_GROUP_ACCESS_TOKEN");
            return;
        }

        logger.LogInformation("VK-REM: старт, интервал проверки {Min} мин", ReminderIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "VK-REM: ошибка цикла");
            }

            await Task.Delay(TimeSpan.FromMinutes(ReminderIntervalMinutes), stoppingToken);
        }
    }

    async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;

        // От 1 ч до 24 ч до дедлайна
        await SendForWindowAsync(db, now, "before_24h",
            h => h.Deadline != null && h.Deadline > now.AddHours(1) && h.Deadline <= now.AddHours(24),
            hw => $"⏳ До дедлайна по «{hw.Title}» меньше суток: {hw.Deadline:dd.MM.yyyy HH:mm} UTC.\nСдай работу на сайте U.F.B., пока не поздно.",
            ct);

        // Последний час
        await SendForWindowAsync(db, now, "before_1h",
            h => h.Deadline != null && h.Deadline > now && h.Deadline <= now.AddHours(1),
            hw => $"⚠️ Час до дедлайна: «{hw.Title}» ({hw.Deadline:dd.MM HH:mm} UTC).\nУспей отправить сдачу на U.F.B.",
            ct);
    }

    async Task SendForWindowAsync(
        AppDbContext db,
        DateTime now,
        string kind,
        System.Linq.Expressions.Expression<Func<Homework, bool>> predicate,
        Func<Homework, string> buildText,
        CancellationToken ct)
    {
        var homeworks = await db.Homeworks.AsNoTracking()
            .Where(predicate)
            .ToListAsync(ct);
        if (homeworks.Count == 0)
            return;

        foreach (var hw in homeworks)
        {
            var submittedIds = await db.Submissions.AsNoTracking()
                .Where(s => s.HomeworkId == hw.Id)
                .Select(s => s.StudentId)
                .Distinct()
                .ToListAsync(ct);

            var students = await db.Users.AsNoTracking()
                .Where(u => u.Role == UserRole.student
                    && u.VkUserId != null
                    && u.VkUserId != ""
                    && !submittedIds.Contains(u.Id))
                .Select(u => new { u.Id, u.VkUserId })
                .ToListAsync(ct);

            foreach (var s in students)
            {
                var already = await db.VkDeadlineReminderLogs.AsNoTracking()
                    .AnyAsync(r => r.StudentId == s.Id && r.HomeworkId == hw.Id && r.Kind == kind, ct);
                if (already)
                    continue;

                var text = buildText(hw);
                var ok = await vk.TrySendUserMessageAsync(s.VkUserId!, text, ct);
                if (!ok)
                    continue;

                db.VkDeadlineReminderLogs.Add(new VkDeadlineReminderLog
                {
                    StudentId = s.Id,
                    HomeworkId = hw.Id,
                    Kind = kind,
                    SentAt = DateTime.UtcNow,
                });
                try
                {
                    await db.SaveChangesAsync(ct);
                }
                catch (DbUpdateException)
                {
                    // гонка / дубликат уникального индекса
                }

                logger.LogInformation("VK-REM: отправлено {Kind} student={StudentId} homework={HwId}", kind, s.Id, hw.Id);
            }
        }
    }
}
