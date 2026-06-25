using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Admin;
using sabmero.DTOs.Order;
using sabmero.DTOs.Service;
using sabmero.Services;

namespace sabmero.Controllers;

// ── ADMIN PANEL ENDPOINTS (all require Admin role) ────────────────────────────
//  GET    /api/admin/dashboard                      → summary numbers
//  GET    /api/admin/users?role=&search=            → list/search users
//  POST   /api/admin/staff                          → create technician/rider
//  PUT    /api/admin/users/{id}/active              → activate / deactivate user
//  PUT    /api/admin/users/{id}/kyc                 → approve / reject KYC (+reason)
//
//  GET    /api/admin/vendor-requests?status=        → list vendor requests
//  PUT    /api/admin/vendor-requests/{id}/review    → approve / reject request (+reason)
//  GET    /api/admin/vendors?onlyPending=           → list approved vendors
//  PUT    /api/admin/vendors/{id}/approval          → (legacy) toggle a profile's approval
//
//  GET    /api/admin/orders?status=                 → list all orders
//  PUT    /api/admin/orders/{id}/assign-rider       → assign rider to order
//
//  GET    /api/admin/bookings?status=               → list all bookings
//  PUT    /api/admin/bookings/{id}/assign-tech      → assign technician
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    private readonly IVendorService _vendors;
    private readonly IOrderService _orders;
    private readonly IServiceBookingService _bookings;

    public AdminController(
        IAdminService admin,
        IVendorService vendors,
        IOrderService orders,
        IServiceBookingService bookings)
    {
        _admin = admin;
        _vendors = vendors;
        _orders = orders;
        _bookings = bookings;
    }

    // ── DASHBOARD ──────────────────────────────────────────────────────────
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
        => Ok(new { success = true, data = await _admin.GetDashboardAsync() });

    // ── USERS ──────────────────────────────────────────────────────────────
    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] string? role, [FromQuery] string? search)
        => Ok(new { success = true, data = await _admin.GetUsersAsync(role, search) });

    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _admin.CreateStaffAsync(dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    // Create a brand-new vendor: user account + approved Vendor profile in one step.
    [HttpPost("vendors")]
    public async Task<IActionResult> CreateVendor([FromBody] CreateVendorAccountDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _admin.CreateVendorAccountAsync(dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [HttpPut("users/{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetActiveDto dto)
    {
        var (success, message) = await _admin.SetUserActiveAsync(id, dto.IsActive);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    // Approve or reject a user's KYC. On rejection, RejectionReason is required
    // and is surfaced to the user via GET /api/Auth/kyc-status.
    [HttpPut("users/{id:int}/kyc")]
    public async Task<IActionResult> ReviewKyc(int id, [FromBody] ReviewKycDto dto)
    {
        var (success, message) = await _admin.ReviewKycAsync(id, dto.Verified, dto.RejectionReason);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    // ── VENDOR REQUESTS ────────────────────────────────────────────────────
    // The correct onboarding flow: review a pending REQUEST. Approval creates
    // the Vendor profile and upgrades the user's role; rejection stores a reason.
    [HttpGet("vendor-requests")]
    public async Task<IActionResult> VendorRequests([FromQuery] string? status)
        => Ok(new { success = true, data = await _vendors.GetRequestsAsync(status) });

    [HttpPut("vendor-requests/{id:int}/review")]
    public async Task<IActionResult> ReviewVendorRequest(int id, [FromBody] ReviewVendorRequestDto dto)
    {
        var (success, message) = await _vendors.ReviewRequestAsync(
            id, dto.Approved, dto.CommissionRate, dto.RejectionReason);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    // ── VENDORS (approved profiles) ────────────────────────────────────────
    [HttpGet("vendors")]
    public async Task<IActionResult> Vendors([FromQuery] bool onlyPending = false)
        => Ok(new { success = true, data = await _vendors.GetAllAsync(onlyPending) });

    // Legacy: toggle approval directly on a Vendor profile that already exists.
    [HttpPut("vendors/{id:int}/approval")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveVendorDto dto)
    {
        var (success, message) = await _vendors.SetApprovalAsync(id, dto.Approved, dto.CommissionRate);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    // ── ORDERS ─────────────────────────────────────────────────────────────
    [HttpGet("orders")]
    public async Task<IActionResult> Orders([FromQuery] string? status)
        => Ok(new { success = true, data = await _orders.GetAllAsync(status) });

    [HttpPut("orders/{id:int}/assign-rider")]
    public async Task<IActionResult> AssignRider(int id, [FromBody] AssignRiderDto dto)
    {
        var (success, message) = await _orders.AssignRiderAsync(id, dto.RiderId);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    // ── BOOKINGS ───────────────────────────────────────────────────────────
    [HttpGet("bookings")]
    public async Task<IActionResult> Bookings([FromQuery] string? status)
        => Ok(new { success = true, data = await _bookings.GetAllAsync(status) });

    [HttpPut("bookings/{id:int}/assign-tech")]
    public async Task<IActionResult> AssignTech(int id, [FromBody] AssignTechnicianDto dto)
    {
        var (success, message) = await _bookings.AssignTechnicianAsync(id, dto.TechnicianId);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }
}
