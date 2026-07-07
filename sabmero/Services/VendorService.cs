using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Vendor;
using sabmero.Models;

namespace sabmero.Services;

// Handles vendor onboarding and admin approval.
//
// Correct flow (profile created ONLY after approval):
//   1. A logged-in user applies      → creates a VendorRequest (Pending).
//                                        The user's role is NOT changed yet.
//   2. Admin approves the request     → a Vendor profile is created, the user's
//                                        role becomes "Vendor", request → Approved.
//   3. Admin rejects the request      → no profile created, request → Rejected
//                                        with a RejectionReason returned to the user.
//
// The applicant can poll GET /api/Vendors/request-status to see Pending /
// Approved / Rejected (+ reason).
public class VendorService : IVendorService
{
    private readonly AppDbContext _db;
    private readonly ILogger<VendorService> _logger;

    public VendorService(AppDbContext db, ILogger<VendorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, VendorRequestDto? Data)> ApplyAsync(int userId, CreateVendorDto dto)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found.", null);

        // Already an approved vendor?
        bool alreadyVendor = await _db.Vendors.AnyAsync(v => v.UserId == userId);
        if (alreadyVendor)
            return (false, "You already have a vendor profile.", null);

        // Block duplicate open requests. A user may re-apply only if their last
        // request was Rejected.
        var existing = await _db.VendorRequests
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (existing != null && existing.Status == "Pending")
            return (false, "Your vendor request is already pending admin review.", null);
        if (existing != null && existing.Status == "Approved")
            return (false, "You are already an approved vendor.", null);

        var request = new VendorRequest
        {
            UserId = userId,
            BusinessName = dto.BusinessName.Trim(),
            BusinessAddress = dto.BusinessAddress.Trim(),
            BusinessDocumentPath = dto.BusinessDocumentPath,
            CitizenshipDocumentPath = dto.CitizenshipDocumentPath,
            NidDocumentPath = dto.NidDocumentPath,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.VendorRequests.Add(request);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Vendor request {Id} submitted by user {UserId}", request.Id, userId);

        return (true, "Vendor application submitted. Waiting for admin approval.",
            await GetRequestDtoAsync(request.Id));
    }

    // ── PUBLIC self-registration (no login) ──
    // Creates the User account AND a Pending VendorRequest with all three docs.
    // The Vendor profile + role upgrade still happen only on admin approval.
    public async Task<(bool Success, string Message, VendorRequestDto? Data)> RegisterAsync(RegisterVendorDto dto)
    {
        var phone = dto.Phone.Trim();
        bool phoneTaken = await _db.Users.AnyAsync(u => u.Phone == phone);
        if (phoneTaken)
            return (false, "This phone number is already registered. Please log in and apply instead.", null);

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Phone = phone,
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email!.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Address = dto.Address.Trim(),
            // Stays a Customer until an admin approves the request.
            Role = "Customer",
            IsActive = true,
            IsKycVerified = false,
            KycStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var request = new VendorRequest
        {
            UserId = user.Id,
            BusinessName = dto.BusinessName.Trim(),
            BusinessAddress = dto.BusinessAddress.Trim(),
            BusinessDocumentPath = dto.BusinessDocumentPath,
            CitizenshipDocumentPath = dto.CitizenshipDocumentPath,
            NidDocumentPath = dto.NidDocumentPath,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _db.VendorRequests.Add(request);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Public vendor registration: user {UserId}, request {ReqId}", user.Id, request.Id);
        return (true, "Registration submitted. An admin will review your documents.",
            await GetRequestDtoAsync(request.Id));
    }

    // The applicant's latest request status (Pending / Approved / Rejected + reason).
    public async Task<VendorRequestDto?> GetMyRequestAsync(int userId)
    {
        var request = await _db.VendorRequests
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        return request == null ? null : await GetRequestDtoAsync(request.Id);
    }

    public async Task<List<VendorRequestDto>> GetRequestsAsync(string? status)
    {
        var q = _db.VendorRequests.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(r => r.Status == status);

        var ids = await q.OrderByDescending(r => r.CreatedAt).Select(r => r.Id).ToListAsync();
        var list = new List<VendorRequestDto>();
        foreach (var id in ids)
        {
            var dto = await GetRequestDtoAsync(id);
            if (dto != null) list.Add(dto);
        }
        return list;
    }

    // Admin approves or rejects a vendor REQUEST. The Vendor profile is created
    // here, on approval — never before.
    public async Task<(bool Success, string Message)> ReviewRequestAsync(
        int requestId, bool approved, decimal? commissionRate, string? rejectionReason)
    {
        var request = await _db.VendorRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
            return (false, "Vendor request not found.");
        if (request.Status != "Pending")
            return (false, $"This request has already been {request.Status.ToLower()}.");

        var user = await _db.Users.FindAsync(request.UserId);
        if (user == null)
            return (false, "Applicant account no longer exists.");

        if (!approved)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
                return (false, "A rejection reason is required when rejecting a vendor request.");

            request.Status = "Rejected";
            request.RejectionReason = rejectionReason.Trim();
            request.ReviewedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Vendor request {Id} rejected", requestId);
            return (true, "Vendor request rejected.");
        }

        // ── Approve: create the Vendor profile now ──
        var vendor = new Vendor
        {
            UserId = request.UserId,
            BusinessName = request.BusinessName,
            BusinessAddress = request.BusinessAddress,
            BusinessDocumentPath = request.BusinessDocumentPath,
            IsApproved = true,
            CommissionRate = commissionRate ?? 10.0m,
            CreatedAt = DateTime.UtcNow
        };
        _db.Vendors.Add(vendor);

        // Upgrade the user's role only now.
        user.Role = "Vendor";

        request.Status = "Approved";
        request.RejectionReason = null;
        request.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Link the created profile back to the request.
        request.VendorId = vendor.Id;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Vendor request {Id} approved → vendor {VendorId}", requestId, vendor.Id);
        return (true, "Vendor approved and profile created.");
    }

    public async Task<VendorDto?> GetByUserIdAsync(int userId)
        => await BuildQuery().Where(v => v.UserId == userId).FirstOrDefaultAsync();

    public async Task<VendorDto?> GetByIdAsync(int vendorId)
        => await BuildQuery().Where(v => v.Id == vendorId).FirstOrDefaultAsync();

    public async Task<List<VendorDto>> GetAllAsync(bool onlyPending)
    {
        // "Pending vendors" now means pending REQUESTS, but this method is kept
        // for the existing approved-vendor listing. onlyPending returns vendors
        // not yet approved (should normally be empty under the new flow).
        var q = BuildQuery();
        if (onlyPending)
            q = q.Where(v => !v.IsApproved);
        return await q.ToListAsync();
    }

    // Legacy direct-on-profile approval (kept so the old route still compiles).
    public async Task<(bool Success, string Message)> SetApprovalAsync(int vendorId, bool approved, decimal? commissionRate)
    {
        var vendor = await _db.Vendors.FindAsync(vendorId);
        if (vendor == null)
            return (false, "Vendor not found.");

        vendor.IsApproved = approved;
        if (commissionRate.HasValue)
            vendor.CommissionRate = commissionRate.Value;

        await _db.SaveChangesAsync();
        return (true, approved ? "Vendor approved." : "Vendor approval revoked.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private async Task<VendorRequestDto?> GetRequestDtoAsync(int requestId)
    {
        var r = await _db.VendorRequests
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == requestId);
        if (r == null) return null;

        return new VendorRequestDto
        {
            Id = r.Id,
            UserId = r.UserId,
            OwnerName = r.User.FullName,
            Phone = r.User.Phone,
            BusinessName = r.BusinessName,
            BusinessAddress = r.BusinessAddress,
            BusinessDocumentPath = r.BusinessDocumentPath,
            CitizenshipDocumentPath = r.CitizenshipDocumentPath,
            NidDocumentPath = r.NidDocumentPath,
            Status = r.Status,
            RejectionReason = r.RejectionReason,
            VendorId = r.VendorId,
            CreatedAt = r.CreatedAt,
            ReviewedAt = r.ReviewedAt
        };
    }

    // Shared projection so all vendor-profile reads return the same shape.
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
