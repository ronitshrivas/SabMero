using sabmero.DTOs.Admin;

namespace sabmero.Services;

// Contract for admin-only operations: dashboard, user & staff management.
public interface IAdminService
{
    Task<DashboardStatsDto> GetDashboardAsync();
    Task<List<AdminUserDto>> GetUsersAsync(string? role, string? search);
    Task<(bool Success, string Message, AdminUserDto? Data)> CreateStaffAsync(CreateStaffDto dto);
    Task<(bool Success, string Message)> SetUserActiveAsync(int userId, bool isActive);

    // Approve/reject KYC. rejectionReason is required when verified == false.
    Task<(bool Success, string Message)> ReviewKycAsync(int userId, bool verified, string? rejectionReason);

    // Create a brand-new vendor (user account + approved profile) in one step.
    Task<(bool Success, string Message, AdminUserDto? Data)> CreateVendorAccountAsync(CreateVendorAccountDto dto);
}