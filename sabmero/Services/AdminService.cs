using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Admin;
using sabmero.Models;

namespace sabmero.Services;

// Powers the Admin Panel: dashboard numbers, listing/searching users,
// creating staff accounts (technicians & riders), and (de)activating users.
public class AdminService : IAdminService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AdminService> _logger;

    public AdminService(AppDbContext db, ILogger<AdminService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardAsync()
    {
        // Delivered orders drive the sales & commission totals.
        var deliveredTotals = await _db.Orders
            .Where(o => o.Status == "Delivered")
            .Select(o => new { o.TotalAmount, o.CommissionAmount })
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalUsers = await _db.Users.CountAsync(),
            TotalCustomers = await _db.Users.CountAsync(u => u.Role == "Customer"),
            TotalVendors = await _db.Users.CountAsync(u => u.Role == "Vendor"),
            PendingVendors = await _db.Vendors.CountAsync(v => !v.IsApproved),
            TotalTechnicians = await _db.Users.CountAsync(u => u.Role == "Technician"),
            TotalRiders = await _db.Users.CountAsync(u => u.Role == "Rider"),

            TotalProducts = await _db.Products.CountAsync(p => p.IsActive),
            TotalCategories = await _db.Categories.CountAsync(),

            TotalOrders = await _db.Orders.CountAsync(),
            PendingOrders = await _db.Orders.CountAsync(o => o.Status == "Pending"),
            DeliveredOrders = await _db.Orders.CountAsync(o => o.Status == "Delivered"),

            TotalBookings = await _db.ServiceBookings.CountAsync(),
            PendingBookings = await _db.ServiceBookings.CountAsync(b => b.Status == "Pending"),

            TotalSales = deliveredTotals.Sum(x => x.TotalAmount),
            TotalCommission = deliveredTotals.Sum(x => x.CommissionAmount),
            PendingReturns = await _db.ReturnRequests.CountAsync(r => r.Status == "Pending")
        };
    }

    public async Task<List<AdminUserDto>> GetUsersAsync(string? role, string? search)
    {
        var q = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
            q = q.Where(u => u.Role == role);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(u => u.FullName.ToLower().Contains(s) || u.Phone.Contains(s));
        }

        return await q
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Phone = u.Phone,
                Email = u.Email,
                Role = u.Role,
                IsKycVerified = u.IsKycVerified,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, AdminUserDto? Data)> CreateStaffAsync(CreateStaffDto dto)
    {
        if (dto.Role != "Technician" && dto.Role != "Rider")
            return (false, "Staff role must be Technician or Rider.", null);

        bool exists = await _db.Users.AnyAsync(u => u.Phone == dto.Phone);
        if (exists)
            return (false, "This phone number is already registered.", null);

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Phone = dto.Phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Address = dto.Address?.Trim() ?? string.Empty,
            Role = dto.Role,
            IsActive = true,
            IsKycVerified = true,    // staff are vetted by admin directly
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Staff account created: {Phone} as {Role}", user.Phone, user.Role);

        return (true, $"{dto.Role} account created.", new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Phone = user.Phone,
            Email = user.Email,
            Role = user.Role,
            IsKycVerified = user.IsKycVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }

    public async Task<(bool Success, string Message)> SetUserActiveAsync(int userId, bool isActive)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found.");
        if (user.Role == "Admin")
            return (false, "Admin accounts cannot be deactivated.");

        user.IsActive = isActive;
        await _db.SaveChangesAsync();
        return (true, isActive ? "User activated." : "User deactivated.");
    }

    public async Task<(bool Success, string Message)> VerifyKycAsync(int userId, bool verified)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found.");

        user.IsKycVerified = verified;
        await _db.SaveChangesAsync();
        return (true, verified ? "KYC verified." : "KYC verification removed.");
    }
}