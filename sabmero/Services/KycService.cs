using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Auth;

namespace sabmero.Services;

// Handles the user-facing side of KYC: submitting a document and checking
// status. Admin approval/rejection lives in AdminService.
public class KycService : IKycService
{
    private readonly AppDbContext _db;
    private readonly ILogger<KycService> _logger;

    public KycService(AppDbContext db, ILogger<KycService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Submit (or re-submit) a KYC document. Moves the user to "Pending" and
    // clears any previous rejection reason. Re-submission is allowed after a
    // rejection so the user can fix and try again.
    public async Task<(bool Success, string Message, KycStatusDto? Data)> SubmitAsync(int userId, SubmitKycDto dto)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found.", null);

        if (user.KycStatus == "Approved")
            return (false, "Your KYC is already approved.", null);

        user.KycDocumentPath = dto.DocumentPath;
        user.KycStatus = "Pending";
        user.KycRejectionReason = null;
        user.IsKycVerified = false;

        await _db.SaveChangesAsync();
        _logger.LogInformation("KYC submitted by user {UserId}", userId);

        return (true, "KYC submitted. Waiting for admin review.", Map(user.KycStatus, user.KycRejectionReason, user.KycDocumentPath, user.IsKycVerified));
    }

    public async Task<KycStatusDto?> GetStatusAsync(int userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        return Map(user.KycStatus, user.KycRejectionReason, user.KycDocumentPath, user.IsKycVerified);
    }

    private static KycStatusDto Map(string status, string? reason, string? doc, bool verified)
        => new KycStatusDto
        {
            Status = status,
            RejectionReason = status == "Rejected" ? reason : null,
            DocumentPath = doc,
            IsVerified = verified
        };
}
