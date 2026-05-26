using HackBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HackBackend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Homework> Homeworks => Set<Homework>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<SubmissionItem> SubmissionItems => Set<SubmissionItem>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<VkDeadlineReminderLog> VkDeadlineReminderLogs => Set<VkDeadlineReminderLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        b.Entity<Homework>(e =>
        {
            e.HasOne(h => h.Author)
             .WithMany(u => u.CreatedHomeworks)
             .HasForeignKey(h => h.CreatedBy);
        });

        b.Entity<Submission>(e =>
        {
            e.Property(s => s.Status).HasConversion<string>();
            e.HasOne(s => s.Homework).WithMany(h => h.Submissions).HasForeignKey(s => s.HomeworkId);
            e.HasOne(s => s.Student).WithMany(u => u.Submissions).HasForeignKey(s => s.StudentId);
            e.HasOne(s => s.Grade).WithOne(g => g.Submission).HasForeignKey<Grade>(g => g.SubmissionId);
        });

        b.Entity<SubmissionItem>(e =>
        {
            e.Property(i => i.Type).HasConversion<string>();
            e.HasOne(i => i.Submission).WithMany(s => s.Items).HasForeignKey(i => i.SubmissionId);
        });

        b.Entity<Grade>(e =>
        {
            e.HasOne(g => g.Teacher).WithMany(u => u.GradesGiven).HasForeignKey(g => g.TeacherId);
        });

        b.Entity<VkDeadlineReminderLog>(e =>
        {
            e.Property(x => x.Kind).HasMaxLength(32).IsRequired();
            e.HasIndex(x => new { x.StudentId, x.HomeworkId, x.Kind }).IsUnique();
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Homework).WithMany().HasForeignKey(x => x.HomeworkId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
