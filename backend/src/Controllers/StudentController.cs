using HackBackend.Auth;
using HackBackend.Data;
using HackBackend.DTOs;
using HackBackend.Models;
using HackBackend.Options;
using HackBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackBackend.Controllers;

[ApiController]
[Route("api/student")]
[Authorize(Policy = "student")]
public class StudentController(
    AppDbContext db,
    INotificationService notifications,
    UploadSettings uploads) : ControllerBase
{
    [HttpGet("homeworks")]
    public async Task<IActionResult> GetHomeworks()
    {
        var list = await db.Homeworks
            .OrderBy(h => h.Deadline)
            .Select(h => new HomeworkBriefResponse(h.Id, h.Title, h.Project, h.Direction, h.Course, h.Deadline, h.Description))
            .ToListAsync();
        return Ok(list);
    }

    /// <summary>Домашние задания, по которым студент ещё не отправил ни одной сдачи (раздел «К сдаче»).</summary>
    [HttpGet("homeworks/todo")]
    public async Task<IActionResult> GetHomeworksTodo(CancellationToken ct)
    {
        var studentId = User.UserId();
        var doneHwIds = await db.Submissions
            .AsNoTracking()
            .Where(s => s.StudentId == studentId)
            .Select(s => s.HomeworkId)
            .Distinct()
            .ToListAsync(ct);

        var list = await db.Homeworks
            .AsNoTracking()
            .Where(h => !doneHwIds.Contains(h.Id))
            .OrderBy(h => h.Deadline ?? DateTime.MaxValue)
            .ThenBy(h => h.Id)
            .Select(h => new HomeworkBriefResponse(h.Id, h.Title, h.Project, h.Direction, h.Course, h.Deadline, h.Description))
            .ToListAsync(ct);

        return Ok(list);
    }

    /// <summary>Статистика студента за текущий календарный месяц.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetMonthStats(CancellationToken ct)
    {
        var studentId = User.UserId();
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var subs = await db.Submissions
            .AsNoTracking()
            .Where(s => s.StudentId == studentId && s.SubmittedAt >= monthStart)
            .Select(s => new { s.Status, Score = (int?)s.Grade!.Score })
            .ToListAsync(ct);

        var total  = subs.Count;
        var graded = subs.Count(s => s.Status == SubmissionStatus.graded);

        int? avgScore = graded > 0
            ? (int)Math.Round(subs.Where(s => s.Status == SubmissionStatus.graded && s.Score.HasValue)
                                  .Average(s => (double)s.Score!.Value))
            : null;

        var successPercent = total > 0 ? (int)Math.Round(graded * 100.0 / total) : 0;

        return Ok(new StudentMonthStatsResponse(total, avgScore, successPercent));
    }

    [HttpGet("submissions")]
    public async Task<IActionResult> GetSubmissions([FromQuery] string? status)
    {
        var studentId = User.UserId();
        var q = db.Submissions
            .Include(s => s.Homework)
            .Include(s => s.Grade)
            .Where(s => s.StudentId == studentId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<SubmissionStatus>(status, out var parsed))
            q = q.Where(s => s.Status == parsed);

        var list = await q.OrderByDescending(s => s.SubmittedAt).ToListAsync();

        return Ok(list.Select(s => new SubmissionBriefResponse(
            s.Id,
            s.HomeworkId,
            s.Status.ToString(),
            s.SubmittedAt,
            new HomeworkBriefResponse(s.Homework.Id, s.Homework.Title, s.Homework.Project, s.Homework.Direction, s.Homework.Course, s.Homework.Deadline, s.Homework.Description),
            s.Grade is null ? null : new GradeScoreResponse(s.Grade.Score)
        )));
    }

    [HttpGet("submissions/{id:int}")]
    public async Task<IActionResult> GetSubmission(int id)
    {
        var sub = await db.Submissions
            .Include(s => s.Homework)
            .Include(s => s.Items)
            .Include(s => s.Grade).ThenInclude(g => g!.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id && s.StudentId == User.UserId());

        if (sub is null) return NotFound();

        return Ok(new SubmissionDetailResponse(
            sub.Id,
            sub.Status.ToString(),
            sub.SubmittedAt,
            new HomeworkBriefResponse(sub.Homework.Id, sub.Homework.Title, sub.Homework.Project, sub.Homework.Direction, sub.Homework.Course, sub.Homework.Deadline, sub.Homework.Description),
            sub.Items.Select(i => new SubmissionItemResponse(i.Id, i.Type.ToString(), i.Url, i.OriginalName, i.FileSize)),
            sub.Grade is null ? null : new GradeResponse(sub.Grade.Score, sub.Grade.Comment, sub.Grade.GradedAt, sub.Grade.Teacher.Name)
        ));
    }

    [HttpPost("submissions")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> CreateSubmission(CancellationToken ct)
    {
        var form = await Request.ReadFormAsync(ct);

        if (!int.TryParse(form["homework_id"], out var hwId))
            return BadRequest(new { message = "homework_id required" });

        var hw = await db.Homeworks.FirstOrDefaultAsync(h => h.Id == hwId, ct);
        if (hw is null) return NotFound(new { message = "Задание не найдено" });

        var studentId = User.UserId();
        var hasPending = await db.Submissions.AnyAsync(
            s => s.StudentId == studentId && s.HomeworkId == hwId && s.Status == SubmissionStatus.pending, ct);
        if (hasPending)
            return Conflict(new { message = "По этому заданию уже есть работа на проверке. Отредактируйте её в «Моих сдачах»." });

        var files = form.Files;
        var links = form["links[]"].ToArray();
        if (files.Count == 0 && links.Length == 0)
            return BadRequest(new { message = "Нужен хотя бы один файл или ссылка" });

        var submission = new Submission
        {
            HomeworkId  = hwId,
            StudentId   = studentId,
            Status      = SubmissionStatus.pending,
            SubmittedAt = DateTime.UtcNow,
        };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync(ct);

        foreach (var file in files)
        {
            var dir      = Path.Combine(uploads.Path, submission.Id.ToString());
            Directory.CreateDirectory(dir);
            var safeName = Path.GetFileName(file.FileName);
            var dest     = Path.Combine(dir, safeName);
            await using var fs = System.IO.File.Create(dest);
            await file.CopyToAsync(fs, ct);

            db.SubmissionItems.Add(new SubmissionItem
            {
                SubmissionId = submission.Id,
                Type         = ItemType.file,
                FilePath     = dest,
                OriginalName = safeName,
                MimeType     = file.ContentType,
                FileSize     = file.Length,
            });
        }

        foreach (var link in links.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            db.SubmissionItems.Add(new SubmissionItem
            {
                SubmissionId = submission.Id,
                Type         = ItemType.link,
                Url          = link,
            });
        }
        await db.SaveChangesAsync(ct);

        var student = await db.Users.AsNoTracking()
            .Where(u => u.Id == studentId)
            .Select(u => new { u.Name, u.VkUserId })
            .FirstAsync(ct);

        await notifications.NotifyTeachersNewSubmissionAsync(hw, submission.Id, student.Name, ct);
        await notifications.NotifyStudentSubmissionReceivedAsync(student.VkUserId, hw.Title, submission.Id, ct);

        return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, new { id = submission.Id });
    }

    /// <summary>Обновить материалы сдачи (только статус «на проверке»).</summary>
    [HttpPatch("submissions/{id:int}")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UpdateSubmission(int id, CancellationToken ct)
    {
        var form = await Request.ReadFormAsync(ct);
        var studentId = User.UserId();

        var sub = await db.Submissions
            .Include(s => s.Items)
            .Include(s => s.Homework)
            .FirstOrDefaultAsync(s => s.Id == id && s.StudentId == studentId, ct);

        if (sub is null)
            return NotFound(new { message = "Сдача не найдена" });
        if (sub.Status != SubmissionStatus.pending)
            return Conflict(new { message = "Можно редактировать только работу на проверке." });

        var keepRaw = form["keep_item_ids"].ToString();
        var keepIds = new HashSet<int>();
        if (!string.IsNullOrWhiteSpace(keepRaw))
        {
            foreach (var part in keepRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (int.TryParse(part, out var pid))
                    keepIds.Add(pid);
        }

        var newFiles = form.Files.Where(f => f.Length > 0).ToList();
        var links = form["links[]"].ToArray();
        var linkCount = links.Count(static l => !string.IsNullOrWhiteSpace(l));

        var keptFiles = sub.Items
            .Where(i => i.Type == ItemType.file && keepIds.Contains(i.Id))
            .ToList();

        if (keptFiles.Count == 0 && newFiles.Count == 0 && linkCount == 0)
            return BadRequest(new { message = "Нужен хотя бы один файл или ссылка" });

        foreach (var item in sub.Items.ToList())
        {
            if (item.Type == ItemType.file)
            {
                if (keepIds.Contains(item.Id))
                    continue;
                if (!string.IsNullOrEmpty(item.FilePath) && System.IO.File.Exists(item.FilePath))
                {
                    try { System.IO.File.Delete(item.FilePath); }
                    catch { /* ignore locked files */ }
                }
                db.SubmissionItems.Remove(item);
            }
            else
                db.SubmissionItems.Remove(item);
        }

        await db.SaveChangesAsync(ct);

        var dir = Path.Combine(uploads.Path, sub.Id.ToString());
        Directory.CreateDirectory(dir);

        foreach (var file in newFiles)
        {
            var safeName = Path.GetFileName(file.FileName);
            var dest = Path.Combine(dir, safeName);
            await using var fs = System.IO.File.Create(dest);
            await file.CopyToAsync(fs, ct);

            db.SubmissionItems.Add(new SubmissionItem
            {
                SubmissionId = sub.Id,
                Type         = ItemType.file,
                FilePath     = dest,
                OriginalName = safeName,
                MimeType     = file.ContentType,
                FileSize     = file.Length,
            });
        }

        foreach (var link in links.Where(static l => !string.IsNullOrWhiteSpace(l)))
        {
            db.SubmissionItems.Add(new SubmissionItem
            {
                SubmissionId = sub.Id,
                Type         = ItemType.link,
                Url          = link,
            });
        }

        sub.SubmittedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(new { id = sub.Id });
    }
}
