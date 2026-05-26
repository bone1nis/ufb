namespace HackBackend.Models;

public class Grade
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public int TeacherId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime GradedAt { get; set; } = DateTime.UtcNow;

    public Submission Submission { get; set; } = null!;
    public User Teacher { get; set; } = null!;
}
