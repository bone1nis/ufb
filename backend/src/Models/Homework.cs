namespace HackBackend.Models;

public class Homework
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Project { get; set; } = "";
    public string Direction { get; set; } = "";
    public int Course { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Author { get; set; } = null!;
    public ICollection<Submission> Submissions { get; set; } = [];
}
