using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Auth;

// Flutter app sends this JSON body to POST /api/auth/register
public class RegisterDto
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    // Optional: "Customer" (default) | "Vendor" | "Technician" | "Rider"
    // Note: "Admin" cannot be self-registered
    public string Role { get; set; } = "Customer";
}