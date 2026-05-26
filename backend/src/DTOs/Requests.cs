namespace HackBackend.DTOs;

public record LoginRequest(string Email, string Password);

public record LinkVkRequest(string? VkUserId);

public record GradeRequest(int Score, string? Comment);

public record HomeworkRequest(
    string Title,
    string? Description,
    string Project,
    string Direction,
    int Course,
    DateTime? Deadline);
