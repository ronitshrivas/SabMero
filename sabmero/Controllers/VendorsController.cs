using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Vendor;
using sabmero.Helpers;
using sabmero.Services;

namespace sabmero.Controllers;

// ── VENDOR ENDPOINTS ──────────────────────────────────────────────────────────
//  POST   /api/vendors/apply            → apply to become a vendor   (any logged-in user)
//  GET    /api/vendors/request-status   → my vendor request status   (any logged-in user)
//  GET    /api/vendors/me               → my vendor profile          (Vendor)
//  GET    /api/vendors/{id}             → public vendor profile       (public)
//
// (Admin review lives under /api/admin/vendor-requests — see AdminController.)
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _service;

    public VendorsController(IVendorService service)
    {
        _service = service;
    }

    // PUBLIC self-registration — no login required. Creates the account + a
    // Pending vendor request with all three documents.
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterVendorDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.RegisterAsync(dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [Authorize]
    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] CreateVendorDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.ApplyAsync(User.GetUserId(), dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    // The applicant checks whether their request is Pending / Approved / Rejected.
    // When Rejected, the response includes the rejection reason.
    [Authorize]
    [HttpGet("request-status")]
    public async Task<IActionResult> RequestStatus()
    {
        var data = await _service.GetMyRequestAsync(User.GetUserId());
        if (data == null)
            return NotFound(new { success = false, message = "You have not applied to become a vendor." });
        return Ok(new { success = true, data });
    }

    [Authorize(Roles = "Vendor,Admin")]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var data = await _service.GetByUserIdAsync(User.GetUserId());
        if (data == null)
            return NotFound(new { success = false, message = "No vendor profile found." });
        return Ok(new { success = true, data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data == null)
            return NotFound(new { success = false, message = "Vendor not found." });
        return Ok(new { success = true, data });
    }
}
