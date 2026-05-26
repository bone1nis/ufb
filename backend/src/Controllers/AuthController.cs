using HackBackend.Auth;
using HackBackend.Data;
using HackBackend.DTOs;
using HackBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackBackend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный email или пароль" });

        var token = JwtHelper.Generate(user);
        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite  = SameSiteMode.Lax,
            Secure    = env.IsProduction(),
            Expires   = DateTimeOffset.UtcNow.AddDays(7),
        });
        return Ok(new { user = Map(user) });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var id   = User.UserId();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return Unauthorized();
        return Ok(new { user = Map(user) });
    }

    [HttpPost("vk")]
    [Authorize]
    public async Task<IActionResult> LinkVk([FromBody] LinkVkRequest req)
    {
        var id   = User.UserId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        user.VkUserId = string.IsNullOrWhiteSpace(req.VkUserId) ? null : req.VkUserId.Trim();
        await db.SaveChangesAsync();
        return Ok(new { vk_user_id = user.VkUserId });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return Ok(new { message = "ok" });
    }

    static UserResponse Map(User u) => new(u.Id, u.Name, u.Email, u.Role.ToString(), u.VkUserId);
}
