using HackBackend.Auth;
using HackBackend.Data;
using HackBackend.DTOs;
using HackBackend.Models;
using HackBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackBackend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Policy = "teacher")]
public class TeacherController(AppDbContext db, INotificationService notifications) : ControllerBase
{
    [HttpGet("submissions")]
    public async Task<IActionResult> GetSubmissions([FromQuery] int? homeworkId)
    {
        var q = db.Submissions
            .Include(s => s.Homework)
            .Include(s => s.Student)
            .Include(s => s.Grade)
            .AsQueryable();

        if (homeworkId.HasValue)
            q = q.Where(s => s.HomeworkId == homeworkId.Value);

        var list = await q.OrderByDescending(s => s.SubmittedAt).ToListAsync();

        return Ok(list.Select(s => new SubmissionBriefResponse(
            s.Id,
            s.HomeworkId,
            s.Status.ToString(),
            s.SubmittedAt,
            new HomeworkBriefResponse(s.Homework.Id, s.Homework.Title, s.Homework.Project, s.Homework.Direction, s.Homework.Course, s.Homework.Deadline, s.Homework.Description),
            s.Grade is null ? null : new GradeScoreResponse(s.Grade.Score),
            new StudentBriefResponse(s.Student.Id, s.Student.Name, s.Student.Email)
        )));
    }

    [HttpGet("submissions/{id:int}")]
    public async Task<IActionResult> GetSubmission(int id)
    {
        var sub = await db.Submissions
            .Include(s => s.Homework)
            .Include(s => s.Student)
            .Include(s => s.Items)
            .Include(s => s.Grade).ThenInclude(g => g!.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sub is null) return NotFound();

        return Ok(new SubmissionDetailResponse(
            sub.Id,
            sub.Status.ToString(),
            sub.SubmittedAt,
            new HomeworkBriefResponse(sub.Homework.Id, sub.Homework.Title, sub.Homework.Project, sub.Homework.Direction, sub.Homework.Course, sub.Homework.Deadline, sub.Homework.Description),
            sub.Items.Select(i => new SubmissionItemResponse(i.Id, i.Type.ToString(), i.Url, i.OriginalName, i.FileSize)),
            sub.Grade is null ? null : new GradeResponse(sub.Grade.Score, sub.Grade.Comment, sub.Grade.GradedAt, sub.Grade.Teacher.Name),
            new StudentBriefResponse(sub.Student.Id, sub.Student.Name, sub.Student.Email)
        ));
    }

    [HttpGet("submissions/{id:int}/files/{itemId:int}")]
    public async Task<IActionResult> DownloadFile(int id, int itemId)
    {
        var item = await db.SubmissionItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.SubmissionId == id && i.Type == ItemType.file);

        if (item is null || !System.IO.File.Exists(item.FilePath))
            return NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(item.FilePath!);
        return File(bytes, item.MimeType ?? "application/octet-stream", item.OriginalName);
    }

    [HttpPost("submissions/{id:int}/grade")]
    public async Task<IActionResult> Grade(int id, [FromBody] GradeRequest req, CancellationToken ct)
    {
        var sub = await db.Submissions
            .Include(s => s.Grade)
            .Include(s => s.Student)
            .Include(s => s.Homework)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (sub is null) return NotFound();

        if (sub.Grade is null)
        {
            db.Grades.Add(new Grade
            {
                SubmissionId = id,
                TeacherId    = User.UserId(),
                Score        = req.Score,
                Comment      = req.Comment,
                GradedAt     = DateTime.UtcNow,
            });
        }
        else
        {
            sub.Grade.Score    = req.Score;
            sub.Grade.Comment  = req.Comment;
            sub.Grade.GradedAt = DateTime.UtcNow;
        }

        sub.Status = SubmissionStatus.graded;
        await db.SaveChangesAsync(ct);

        await notifications.NotifyStudentGradedAsync(sub.Student.VkUserId, sub.Homework.Title, req.Score, req.Comment, ct);

        return Ok(new { message = "ok" });
    }
}
