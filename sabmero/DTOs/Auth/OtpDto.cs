using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Auth;

// Flutter app sends this to POST /api/auth/send-otp
// Backend generates a 6-digit code and (in production) sends it via SMS
public class SendOtpDto
{
    [Required(ErrorMessage = "Phone is required")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits")]
    public string Phone { get; set; } = string.Empty;
}

// Flutter app sends this to POST /api/auth/verify-otp
// Backend checks the code and returns a JWT token if correct
public class VerifyOtpDto
{
    [Required(ErrorMessage = "Phone is required")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits")]
    public string Code { get; set; } = string.Empty;
}