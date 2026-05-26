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
[Route("api/methodist")]
[Authorize(Policy = "methodist")]
public class MethodistController(AppDbContext db, INotificationService notifications) : ControllerBase
{
    [HttpGet("homeworks")]
    public async Task<IActionResult> GetHomeworks()
    {
        var list = await db.Homeworks
            .Where(h => h.CreatedBy == User.UserId())
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new HomeworkResponse(h.Id, h.Title, h.Description, h.Project, h.Direction, h.Course, h.Deadline, h.CreatedAt, h.CreatedBy))
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("homeworks")]
    public async Task<IActionResult> CreateHomework([FromBody] HomeworkRequest req, CancellationToken ct)
    {
        var hw = new Homework
        {
            Title       = req.Title,
            Description = req.Description,
            Project     = req.Project,
            Direction   = req.Direction,
            Course      = req.Course,
            CreatedBy   = User.UserId(),
            Deadline    = req.Deadline,
            CreatedAt   = DateTime.UtcNow,
        };
        db.Homeworks.Add(hw);
        await db.SaveChangesAsync(ct);

        await notifications.NotifyTeachersNewHomeworkAsync(hw, ct);

        var resp = new HomeworkResponse(hw.Id, hw.Title, hw.Description, hw.Project, hw.Direction, hw.Course, hw.Deadline, hw.CreatedAt, hw.CreatedBy);
        return CreatedAtAction(nameof(GetHomework), new { id = hw.Id }, resp);
    }

    [HttpGet("homeworks/{id:int}")]
    public async Task<IActionResult> GetHomework(int id)
    {
        var hw = await db.Homeworks
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id && h.CreatedBy == User.UserId());

        if (hw is null)
            return NotFound();

        return Ok(new HomeworkResponse(hw.Id, hw.Title, hw.Description, hw.Project, hw.Direction, hw.Course, hw.Deadline, hw.CreatedAt, hw.CreatedBy));
    }

    [HttpPut("homeworks/{id:int}")]
    public async Task<IActionResult> UpdateHomework(int id, [FromBody] HomeworkRequest req, CancellationToken ct)
    {
        var hw = await db.Homeworks.FirstOrDefaultAsync(h => h.Id == id && h.CreatedBy == User.UserId(), ct);
        if (hw is null)
            return NotFound(new { message = "Задание не найдено" });

        hw.Title = req.Title;
        hw.Description = req.Description;
        hw.Project = req.Project;
        hw.Direction = req.Direction;
        hw.Course = req.Course;
        hw.Deadline = req.Deadline;
        await db.SaveChangesAsync(ct);

        return Ok(new HomeworkResponse(hw.Id, hw.Title, hw.Description, hw.Project, hw.Direction, hw.Course, hw.Deadline, hw.CreatedAt, hw.CreatedBy));
    }
}
