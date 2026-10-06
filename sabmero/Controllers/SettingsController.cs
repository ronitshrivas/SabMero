using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Common;
using sabmero.Services;

namespace sabmero.Controllers;

// ── APP SETTINGS ──────────────────────────────────────────────────────────────
//  GET /api/settings/delivery   → current delivery charge (any logged-in user)
//  PUT /api/settings/delivery   → set delivery charge + free-above threshold (Admin)
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settings;

    public SettingsController(ISettingsService settings)
    {
        _settings = settings;
    }

    // Any logged-in user (customer at checkout) can read the delivery charge.
    [HttpGet("delivery")]
    public async Task<IActionResult> GetDelivery()
        => Ok(new { success = true, data = await _settings.GetDeliveryAsync() });

    [Authorize(Roles = "Admin")]
    [HttpPut("delivery")]
    public async Task<IActionResult> SetDelivery([FromBody] SetDeliverySettingsDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message) = await _settings.SetDeliveryAsync(dto.DeliveryFee, dto.FreeDeliveryAbove);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }
}
