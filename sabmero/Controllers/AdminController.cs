using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Admin;
using sabmero.DTOs.Order;
using sabmero.DTOs.Service;
using sabmero.Services;

namespace sabmero.Controllers;

// ── ADMIN PANEL ENDPOINTS (all require Admin role) ────────────────────────────
//  GET    /api/admin/dashboard                  → summary numbers
//  GET    /api/admin/users?role=&search=        → list/search users
//  POST   /api/admin/staff                      → create technician/rider
//  PUT    /api/admin/users/{id}/active          → activate / deactivate user
//  PUT    /api/admin/users/{id}/kyc             → verify / unverify KYC
//
//  GET    /api/admin/vendors?onlyPending=       → list vendors
//  PUT    /api/admin/vendors/{id}/approval      → approve + set commission
//
//  GET    /api/admin/orders?status=             → list all orders
//  PUT    /api/admin/orders/{id}/assign-rider   → assign rider to order
//
//  GET    /api/admin/bookings?status=           → list all bookings
//  PUT    /api/admin/bookings/{id}/assign-tech  → assign technician
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

    [HttpPut("users/{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetActiveDto dto)
    {
        var (success, message) = await _admin.SetUserActiveAsync(id, dto.IsActive);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    [HttpPut("users/{id:int}/kyc")]
    public async Task<IActionResult> VerifyKyc(int id, [FromBody] SetActiveDto dto)
    {
        // Reuses SetActiveDto's bool: IsActive = "is KYC verified".
        var (success, message) = await _admin.VerifyKycAsync(id, dto.IsActive);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    // ── VENDORS ────────────────────────────────────────────────────────────
    [HttpGet("vendors")]
    public async Task<IActionResult> Vendors([FromQuery] bool onlyPending = false)
        => Ok(new { success = true, data = await _vendors.GetAllAsync(onlyPending) });

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