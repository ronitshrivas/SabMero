using sabmero.DTOs.Admin;

namespace sabmero.Services;

// Contract for admin-only operations: dashboard, user & staff management.
public interface IAdminService
{
    Task<DashboardStatsDto> GetDashboardAsync();
    Task<List<AdminUserDto>> GetUsersAsync(string? role, string? search);
    Task<(bool Success, string Message, AdminUserDto? Data)> CreateStaffAsync(CreateStaffDto dto);
    Task<(bool Success, string Message)> SetUserActiveAsync(int userId, bool isActive);
    Task<(bool Success, string Message)> VerifyKycAsync(int userId, bool verified);
}