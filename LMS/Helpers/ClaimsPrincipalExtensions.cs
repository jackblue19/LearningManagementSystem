using System.Security.Claims;

namespace LMS.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal u)
        => Guid.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static string GetRole(this ClaimsPrincipal u)
        => u.FindFirstValue(ClaimTypes.Role) ?? "student";

    public static string GetEmail(this ClaimsPrincipal u)
        => u.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public static string GetFullName(this ClaimsPrincipal u)
        => u.FindFirst("FullName")?.Value ?? u.Identity?.Name ?? "User";

    public static string GetAvatar(this ClaimsPrincipal u)
        => u.FindFirst("Avatar")?.Value ?? "/images/default-avatar.png";
}

