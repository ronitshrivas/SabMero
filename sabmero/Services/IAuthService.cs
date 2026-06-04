using sabmero.DTOs.Auth;

namespace sabmero.Services;

// Contract for all authentication operations.
// AuthController calls these methods — it doesn't care HOW they work, just that they exist.
public interface IAuthService
{
    Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto dto);
    Task<(bool Success, string Message)> SendOtpAsync(SendOtpDto dto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> VerifyOtpAsync(VerifyOtpDto dto);
}