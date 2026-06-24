using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Auth;
using sabmero.Helpers;
using sabmero.Services;

namespace sabmero.Controllers;

// ── KYC ENDPOINTS (current user) ──────────────────────────────────────────────
//  POST   /api/Auth/kyc          → submit / re-submit a KYC document  (any logged-in)
//  GET    /api/Auth/kyc-status   → check my KYC status + reason        (any logged-in)
//
// Upload the document file via POST /api/Uploads/kyc first, then send the
// returned path to POST /api/Auth/kyc. Admin approves/rejects via
// PUT /api/admin/users/{id}/kyc.
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/Auth")]
[Authorize]
public class KycController : ControllerBase
{
    private readonly IKycService _kyc;

    public KycController(IKycService kyc)
    {
        _kyc = kyc;
    }

    [HttpPost("kyc")]
    public async Task<IActionResult> Submit([FromBody] SubmitKycDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _kyc.SubmitAsync(User.GetUserId(), dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [HttpGet("kyc-status")]
    public async Task<IActionResult> Status()
    {
        var data = await _kyc.GetStatusAsync(User.GetUserId());
        if (data == null)
            return NotFound(new { success = false, message = "User not found." });
        return Ok(new { success = true, data });
    }
}
