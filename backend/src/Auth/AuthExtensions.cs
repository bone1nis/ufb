using System.Security.Claims;

namespace HackBackend.Auth;

public static class AuthExtensions
{
    public static int UserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(sub, out var id) ? id : 0;
    }

    public static string UserRole(this ClaimsPrincipal user) =>
        user.FindFirstValue("role")
        ?? user.FindFirstValue(ClaimTypes.Role)
        ?? user.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
        ?? "";
}
