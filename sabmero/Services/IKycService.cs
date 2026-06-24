using sabmero.DTOs.Auth;

namespace sabmero.Services;

// Contract for the user-facing KYC flow (submit + status check).
public interface IKycService
{
    Task<(bool Success, string Message, KycStatusDto? Data)> SubmitAsync(int userId, SubmitKycDto dto);
    Task<KycStatusDto?> GetStatusAsync(int userId);
}
