namespace HackBackend.Models;

public enum UserRole { student, teacher, methodist }

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.student;
    public string? VkUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Homework> CreatedHomeworks { get; set; } = [];
    public ICollection<Submission> Submissions { get; set; } = [];
    public ICollection<Grade> GradesGiven { get; set; } = [];
}
