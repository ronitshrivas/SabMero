using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Vendor;
using sabmero.Models;

namespace sabmero.Services;

// Handles vendor onboarding and admin approval.
// Flow: User registers (Phase 2) → applies as vendor here → Admin approves → can sell.
public class VendorService : IVendorService
{
    private readonly AppDbContext _db;
    private readonly ILogger<VendorService> _logger;

    public VendorService(AppDbContext db, ILogger<VendorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, VendorDto? Data)> ApplyAsync(int userId, CreateVendorDto dto)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found.", null);

        // One vendor profile per user
        bool already = await _db.Vendors.AnyAsync(v => v.UserId == userId);
        if (already)
            return (false, "You already have a vendor profile.", null);

        var vendor = new Vendor
        {
            UserId = userId,
            BusinessName = dto.BusinessName.Trim(),
            BusinessAddress = dto.BusinessAddress.Trim(),
            BusinessDocumentPath = dto.BusinessDocumentPath,
            IsApproved = false,            // admin must approve
            CommissionRate = 10.0m,        // default, admin can change at approval
            CreatedAt = DateTime.UtcNow
        };

        _db.Vendors.Add(vendor);

        // Upgrade the user's role to Vendor so they get vendor-only endpoints.
        user.Role = "Vendor";

        await _db.SaveChangesAsync();
        _logger.LogInformation("Vendor application submitted by user {UserId}", userId);

        return (true, "Vendor application submitted. Waiting for admin approval.",
            await GetByUserIdAsync(userId));
    }

    public async Task<VendorDto?> GetByUserIdAsync(int userId)
        => await BuildQuery().Where(v => v.UserId == userId).FirstOrDefaultAsync();

    public async Task<VendorDto?> GetByIdAsync(int vendorId)
        => await BuildQuery().Where(v => v.Id == vendorId).FirstOrDefaultAsync();

    public async Task<List<VendorDto>> GetAllAsync(bool onlyPending)
    {
        var q = BuildQuery();
        if (onlyPending)
            q = q.Where(v => !v.IsApproved);
        return await q.ToListAsync();
    }

    public async Task<(bool Success, string Message)> SetApprovalAsync(int vendorId, bool approved, decimal? commissionRate)
    {
        var vendor = await _db.Vendors.FindAsync(vendorId);
        if (vendor == null)
            return (false, "Vendor not found.");

        vendor.IsApproved = approved;
        if (commissionRate.HasValue)
            vendor.CommissionRate = commissionRate.Value;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Vendor {VendorId} approval set to {Approved}", vendorId, approved);

        return (true, approved ? "Vendor approved." : "Vendor approval revoked.");
    }

    // Shared projection so all reads return the same shape.
    private IQueryable<VendorDto> BuildQuery()
        => _db.Vendors.Select(v => new VendorDto
        {
            Id = v.Id,
            UserId = v.UserId,
            OwnerName = v.User.FullName,
            Phone = v.User.Phone,
            BusinessName = v.BusinessName,
            BusinessAddress = v.BusinessAddress,
            BusinessDocumentPath = v.BusinessDocumentPath,
            IsApproved = v.IsApproved,
            CommissionRate = v.CommissionRate,
            ProductCount = v.Products.Count,
            CreatedAt = v.CreatedAt
        });
}