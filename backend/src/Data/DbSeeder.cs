using HackBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HackBackend.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            var users = new[]
            {
                new User { Name = "Студент Тест",  Email = "student@demo.ru",   Role = UserRole.student,
                           PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") },
                new User { Name = "Преподаватель", Email = "teacher@demo.ru",   Role = UserRole.teacher,
                           PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") },
                new User { Name = "Методист",      Email = "methodist@demo.ru", Role = UserRole.methodist,
                           PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") },
            };
            db.Users.AddRange(users);
            await db.SaveChangesAsync();

            var methodist = users[2];
            db.Homeworks.AddRange(
                new Homework { Title = "Верстка лендинга",     Project = "КОД",  Direction = "frontend", Course = 2,
                               Description = "Сверстать по макету Figma. Адаптив обязателен.",
                               CreatedBy = methodist.Id, Deadline = DateTime.UtcNow.AddDays(7) },
                new Homework { Title = "REST API авторизации", Project = "КОД",  Direction = "backend",  Course = 2,
                               Description = "Реализовать регистрацию и логин с JWT.",
                               CreatedBy = methodist.Id, Deadline = DateTime.UtcNow.AddDays(10) },
                new Homework { Title = "UX аудит сервиса",     Project = "ПАЗЛ", Direction = "ux-ui",    Course = 2,
                               Description = "Провести UX-анализ образовательного сервиса.",
                               CreatedBy = methodist.Id, Deadline = DateTime.UtcNow.AddDays(5) }
            );
            await db.SaveChangesAsync();
        }

        await SeedDemoSubmissionsIfEmptyAsync(db);
    }

    static async Task SeedDemoSubmissionsIfEmptyAsync(AppDbContext db)
    {
        if (await db.Submissions.AnyAsync()) return;

        var student = await db.Users.FirstOrDefaultAsync(u => u.Email == "student@demo.ru");
        var teacher = await db.Users.FirstOrDefaultAsync(u => u.Email == "teacher@demo.ru");
        var hws = await db.Homeworks.OrderBy(h => h.Id).Take(2).ToListAsync();
        if (student is null || hws.Count == 0) return;

        var subPending = new Submission
        {
            HomeworkId = hws[0].Id,
            StudentId  = student.Id,
            Status     = SubmissionStatus.pending,
            SubmittedAt = DateTime.UtcNow.AddDays(-2),
            CreatedAt   = DateTime.UtcNow.AddDays(-2),
        };
        db.Submissions.Add(subPending);
        await db.SaveChangesAsync();

        db.SubmissionItems.Add(new SubmissionItem
        {
            SubmissionId = subPending.Id,
            Type = ItemType.link,
            Url  = "https://github.com/example/hacklearn-demo",
        });

        if (hws.Count >= 2 && teacher is not null)
        {
            var subGraded = new Submission
            {
                HomeworkId  = hws[1].Id,
                StudentId   = student.Id,
                Status      = SubmissionStatus.graded,
                SubmittedAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt   = DateTime.UtcNow.AddDays(-5),
            };
            db.Submissions.Add(subGraded);
            await db.SaveChangesAsync();

            db.SubmissionItems.Add(new SubmissionItem
            {
                SubmissionId = subGraded.Id,
                Type = ItemType.link,
                Url  = "https://github.com/example/auth-lab",
            });

            db.Grades.Add(new Grade
            {
                SubmissionId = subGraded.Id,
                TeacherId    = teacher.Id,
                Score        = 88,
                Comment      = "Демо-оценка после сброса БД",
                GradedAt     = DateTime.UtcNow.AddDays(-4),
            });
        }

        await db.SaveChangesAsync();
    }
}
