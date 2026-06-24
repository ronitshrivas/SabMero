using sabmero.DTOs.Vendor;

namespace sabmero.Services;

// Contract for vendor onboarding & approval.
public interface IVendorService
{
    // Apply → creates a Pending VendorRequest (no profile, no role change yet).
    Task<(bool Success, string Message, VendorRequestDto? Data)> ApplyAsync(int userId, CreateVendorDto dto);

    // Applicant polls their latest request status (Pending/Approved/Rejected + reason).
    Task<VendorRequestDto?> GetMyRequestAsync(int userId);

    // Admin lists vendor requests (optionally filtered by status).
    Task<List<VendorRequestDto>> GetRequestsAsync(string? status);

    // Admin approves/rejects a request. Profile is created on approval only.
    Task<(bool Success, string Message)> ReviewRequestAsync(
        int requestId, bool approved, decimal? commissionRate, string? rejectionReason);

    // Vendor-profile reads.
    Task<VendorDto?> GetByUserIdAsync(int userId);
    Task<VendorDto?> GetByIdAsync(int vendorId);
    Task<List<VendorDto>> GetAllAsync(bool onlyPending);

    // Legacy direct-on-profile approval (kept for the existing route).
    Task<(bool Success, string Message)> SetApprovalAsync(int vendorId, bool approved, decimal? commissionRate);
}
