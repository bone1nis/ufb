using HackBackend.Models;

namespace HackBackend.Services;

public interface INotificationService
{
    Task NotifyTeachersNewSubmissionAsync(Homework hw, int submissionId, string studentName, CancellationToken ct = default);
    Task NotifyTeachersNewHomeworkAsync(Homework hw, CancellationToken ct = default);
    Task NotifyStudentSubmissionReceivedAsync(string? vkUserId, string hwTitle, int submissionId, CancellationToken ct = default);
    Task NotifyStudentGradedAsync(string? vkUserId, string hwTitle, int score, string? comment, CancellationToken ct = default);
}
