namespace HackBackend.Models;

public enum SubmissionStatus { pending, graded }

public class Submission
{
    public int Id { get; set; }
    public int HomeworkId { get; set; }
    public int StudentId { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.pending;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Homework Homework { get; set; } = null!;
    public User Student { get; set; } = null!;
    public ICollection<SubmissionItem> Items { get; set; } = [];
    public Grade? Grade { get; set; }
}
