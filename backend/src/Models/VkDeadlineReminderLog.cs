namespace HackBackend.Models;

/// <summary>Один раз на пару студент–задание–тип, чтобы не спамить messages.send.</summary>
public class VkDeadlineReminderLog
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int HomeworkId { get; set; }
    /// <summary>before_24h | before_1h</summary>
    public string Kind { get; set; } = "";
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public User Student { get; set; } = null!;
    public Homework Homework { get; set; } = null!;
}
