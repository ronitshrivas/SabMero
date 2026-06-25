using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Admin;
using sabmero.Models;

namespace sabmero.Services;

// Powers the Admin Panel: dashboard numbers, listing/searching users,
// creating staff accounts (technicians & riders), (de)activating users,
// and reviewing KYC (approve / reject with a reason).
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
            // Pending vendors now means pending vendor REQUESTS.
            PendingVendors = await _db.VendorRequests.CountAsync(r => r.Status == "Pending"),
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
                KycStatus = u.KycStatus,
                KycRejectionReason = u.KycRejectionReason,
                KycDocumentPath = u.KycDocumentPath,
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
            IsKycVerified = true,        // staff are vetted by admin directly
            KycStatus = "Approved",
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
            KycStatus = user.KycStatus,
            KycRejectionReason = user.KycRejectionReason,
            KycDocumentPath = user.KycDocumentPath,
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

    // Approve or reject a user's KYC. On rejection a reason is required and is
    // stored so the user can see why (via GET /api/Auth/kyc-status).
    public async Task<(bool Success, string Message)> ReviewKycAsync(int userId, bool verified, string? rejectionReason)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found.");

        if (verified)
        {
            user.IsKycVerified = true;
            user.KycStatus = "Approved";
            user.KycRejectionReason = null;

            // If this user is a vendor, approving their KYC also clears them to
            // sell (their Vendor profile becomes approved).
            if (user.Role == "Vendor")
            {
                var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.UserId == user.Id);
                if (vendor != null && !vendor.IsApproved)
                    vendor.IsApproved = true;
            }

            await _db.SaveChangesAsync();
            return (true, "KYC approved.");
        }

        if (string.IsNullOrWhiteSpace(rejectionReason))
            return (false, "A rejection reason is required when rejecting KYC.");

        user.IsKycVerified = false;
        user.KycStatus = "Rejected";
        user.KycRejectionReason = rejectionReason.Trim();
        await _db.SaveChangesAsync();
        return (true, "KYC rejected.");
    }

    // Create a brand-new vendor from scratch: User account + approved Vendor
    // profile in one transaction. Used by the admin "Create vendor" action.
    public async Task<(bool Success, string Message, AdminUserDto? Data)> CreateVendorAccountAsync(CreateVendorAccountDto dto)
    {
        bool exists = await _db.Users.AnyAsync(u => u.Phone == dto.Phone);
        if (exists)
            return (false, "This phone number is already registered.", null);

        await using var tx = await _db.Database.BeginTransactionAsync();

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Phone = dto.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email!.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Address = dto.Address?.Trim() ?? string.Empty,
            Role = "Vendor",
            IsActive = true,
            // Admin-created vendors still go through KYC before they can sell.
            IsKycVerified = false,
            KycStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var vendor = new Vendor
        {
            UserId = user.Id,
            BusinessName = dto.BusinessName.Trim(),
            BusinessAddress = dto.BusinessAddress.Trim(),
            BusinessDocumentPath = dto.BusinessDocumentPath,
            // Not approved to sell until KYC is verified.
            IsApproved = false,
            CommissionRate = dto.CommissionRate ?? 10.0m,
            CreatedAt = DateTime.UtcNow
        };
        _db.Vendors.Add(vendor);
        await _db.SaveChangesAsync();

        await tx.CommitAsync();
        _logger.LogInformation("Admin created vendor account {Phone} (vendor {VendorId}) — pending KYC", user.Phone, vendor.Id);

        return (true, "Vendor account created. The vendor must pass KYC before selling.", new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Phone = user.Phone,
            Email = user.Email,
            Role = user.Role,
            IsKycVerified = user.IsKycVerified,
            KycStatus = user.KycStatus,
            KycRejectionReason = user.KycRejectionReason,
            KycDocumentPath = user.KycDocumentPath,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }
}
