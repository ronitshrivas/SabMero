using sabmero.DTOs.Vendor;

namespace sabmero.Services;

// Contract for vendor operations.
public interface IVendorService
{
    // A logged-in user applies to become a vendor.
    Task<(bool Success, string Message, VendorDto? Data)> ApplyAsync(int userId, CreateVendorDto dto);

    // Get the vendor profile belonging to a given user (their own dashboard).
    Task<VendorDto?> GetByUserIdAsync(int userId);

    // Public: get a single vendor by vendor id.
    Task<VendorDto?> GetByIdAsync(int vendorId);

    // Admin: list all vendors (optionally only those pending approval).
    Task<List<VendorDto>> GetAllAsync(bool onlyPending);

    // Admin: approve a vendor and optionally set their commission rate.
    Task<(bool Success, string Message)> SetApprovalAsync(int vendorId, bool approved, decimal? commissionRate);
}