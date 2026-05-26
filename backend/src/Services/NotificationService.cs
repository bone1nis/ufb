using HackBackend.Data;
using HackBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HackBackend.Services;

public sealed class NotificationService(AppDbContext db, VkMessagesClient vk) : INotificationService
{
    public async Task NotifyTeachersNewSubmissionAsync(Homework hw, int submissionId, string studentName, CancellationToken ct = default)
    {
        if (!vk.IsConfigured) return;
        var teachers = await db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.teacher && u.VkUserId != null && u.VkUserId != "")
            .ToListAsync(ct);
        var text = $"Новая сдача по заданию «{hw.Title}».\nСтудент: {studentName}.\nID сдачи: {submissionId}.";
        foreach (var t in teachers)
            await vk.TrySendUserMessageAsync(t.VkUserId!, text, ct);
    }

    public async Task NotifyTeachersNewHomeworkAsync(Homework hw, CancellationToken ct = default)
    {
        if (!vk.IsConfigured) return;
        var teachers = await db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.teacher && u.VkUserId != null && u.VkUserId != "")
            .ToListAsync(ct);
        var text = $"Новое задание: «{hw.Title}» ({hw.Project} · {hw.Direction}, {hw.Course} курс).";
        foreach (var t in teachers)
            await vk.TrySendUserMessageAsync(t.VkUserId!, text, ct);
    }

    public async Task NotifyStudentSubmissionReceivedAsync(string? vkUserId, string hwTitle, int submissionId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(vkUserId) || !vk.IsConfigured) return;
        await vk.TrySendUserMessageAsync(
            vkUserId,
            $"Сдача «{hwTitle}» отправлена (№{submissionId}). Преподаватели получили уведомление.",
            ct);
    }

    public async Task NotifyStudentGradedAsync(string? vkUserId, string hwTitle, int score, string? comment, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(vkUserId) || !vk.IsConfigured) return;
        var commentPart = string.IsNullOrWhiteSpace(comment) ? "" : $"\nКомментарий: {comment}";
        await vk.TrySendUserMessageAsync(
            vkUserId,
            $"Проверено задание «{hwTitle}».\nОценка: {score} из 100.{commentPart}",
            ct);
    }
}
