namespace HackBackend.Models;

public enum ItemType { file, link }

public class SubmissionItem
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public ItemType Type { get; set; }
    public string? Url { get; set; }
    public string? FilePath { get; set; }
    public string? OriginalName { get; set; }
    public string? MimeType { get; set; }
    public long? FileSize { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Submission Submission { get; set; } = null!;
}
