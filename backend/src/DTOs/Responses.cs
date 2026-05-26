namespace HackBackend.DTOs;

// ── Auth ─────────────────────────────────────────────────────────────────────
public record UserResponse(int Id, string Name, string Email, string Role, string? VkUserId);

// ── Homeworks ─────────────────────────────────────────────────────────────────
public record HomeworkBriefResponse(
    int Id, string Title, string Project, string Direction, int Course, DateTime? Deadline, string? Description = null);

public record HomeworkResponse(
    int Id, string Title, string? Description,
    string Project, string Direction, int Course,
    DateTime? Deadline, DateTime CreatedAt, int CreatedBy);

// ── Submissions ───────────────────────────────────────────────────────────────
public record StudentBriefResponse(int Id, string Name, string Email);

public record GradeScoreResponse(int Score);

public record GradeResponse(int Score, string? Comment, DateTime GradedAt, string? TeacherName = null);

public record SubmissionItemResponse(int Id, string Type, string? Url, string? OriginalName, long? FileSize);

/// <summary>Используется и для студента, и для преподавателя. Student заполнен только в ответах преподавателя.</summary>
public record SubmissionBriefResponse(
    int Id, int HomeworkId, string Status, DateTime SubmittedAt,
    HomeworkBriefResponse Homework,
    GradeScoreResponse? Grade,
    StudentBriefResponse? Student = null);

// ── Student stats ─────────────────────────────────────────────────────────────
public record StudentMonthStatsResponse(
    int SubmissionsThisMonth,
    int? AvgScore,
    int SuccessPercent);

public record SubmissionDetailResponse(
    int Id, string Status, DateTime SubmittedAt,
    HomeworkBriefResponse Homework,
    IEnumerable<SubmissionItemResponse> Items,
    GradeResponse? Grade,
    StudentBriefResponse? Student = null);
