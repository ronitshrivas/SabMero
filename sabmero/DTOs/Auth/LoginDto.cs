using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Auth;

// Flutter app sends this JSON body to POST /api/auth/login
public class LoginDto
{
    [Required(ErrorMessage = "Phone is required")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}