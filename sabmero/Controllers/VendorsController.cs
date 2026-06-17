using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Vendor;
using sabmero.Helpers;
using sabmero.Services;

namespace sabmero.Controllers;

// ── VENDOR ENDPOINTS ──────────────────────────────────────────────────────────
//  POST   /api/vendors/apply        → apply to become a vendor            (any logged-in user)
//  GET    /api/vendors/me           → my vendor profile                   (Vendor)
//  GET    /api/vendors/{id}         → public vendor profile               (public)
//
// (Admin approval lives under /api/admin/vendors — see AdminController.)
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