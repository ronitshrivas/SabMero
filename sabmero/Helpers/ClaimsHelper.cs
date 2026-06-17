using System.Security.Claims;

namespace sabmero.Helpers;

// Small helper so every controller can read the logged-in user's ID and role
// from the JWT token in a single clean line, instead of repeating claim lookups.
public static class ClaimsHelper
{
    // Returns the logged-in user's database Id (from the token).
    // Returns 0 if not found (should never happen on [Authorize] endpoints).
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(raw, out var id) ? id : 0;
    }

    // Returns the logged-in user's role: "Customer" | "Vendor" | "Admin" | "Technician" | "Rider"
    public static string GetRole(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    // Returns the logged-in user's phone number (from the token).
    public static string GetPhone(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.MobilePhone)?.Value ?? string.Empty;
}