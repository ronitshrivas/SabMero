using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Payment;
using sabmero.Helpers;
using sabmero.Services;

namespace sabmero.Controllers;

// ── VENDOR COMMISSION / SETTLEMENT PAYMENTS ───────────────────────────────────
// Admin pays vendors their commission/settlement against the vendor's own QR.
//
//  VENDOR (Roles = Vendor):
//   GET  /api/vendor-payments/my-qr        → my saved payout QR
//   PUT  /api/vendor-payments/my-qr        → set/replace my payout QR
//   GET  /api/vendor-payments/mine         → my QR + total received + history
//   POST /api/vendor-payments/{id}/acknowledge → confirm I received a payment
//
//  ADMIN (Roles = Admin):
//   GET  /api/vendor-payments/vendors      → vendors to pay (QR + totals)
//   POST /api/vendor-payments              → record a payment to a vendor
//   GET  /api/vendor-payments/history?vendorId= → payment history (all / per vendor)
//
// Upload the QR / screenshot image first via POST /api/uploads/payment and send
// the returned path.
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/vendor-payments")]
[Authorize]
public class VendorPaymentsController : ControllerBase
{
    private readonly IVendorPaymentService _service;

    public VendorPaymentsController(IVendorPaymentService service)
    {
        _service = service;
    }

    // ── VENDOR ──────────────────────────────────────────────────────────────
    [Authorize(Roles = "Vendor")]
    [HttpGet("my-qr")]
    public async Task<IActionResult> GetMyQr()
    {
        var data = await _service.GetMyQrAsync(User.GetUserId());
        if (data == null)
            return NotFound(new { success = false, message = "No vendor profile found." });
        return Ok(new { success = true, data });
    }

    [Authorize(Roles = "Vendor")]
    [HttpPut("my-qr")]
    public async Task<IActionResult> SetMyQr([FromBody] SetVendorQrDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message) = await _service.SetMyQrAsync(User.GetUserId(), dto.QrImagePath);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    [Authorize(Roles = "Vendor")]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var data = await _service.GetMyPayoutsAsync(User.GetUserId());
        if (data == null)
            return NotFound(new { success = false, message = "No vendor profile found." });
        return Ok(new { success = true, data });
    }

    [Authorize(Roles = "Vendor")]
    [HttpPost("{id:int}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var (success, message) = await _service.AcknowledgeAsync(User.GetUserId(), id);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    // ── ADMIN ───────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    [HttpGet("vendors")]
    public async Task<IActionResult> Vendors()
        => Ok(new { success = true, data = await _service.GetVendorsForPayoutAsync() });

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Record([FromBody] RecordVendorPaymentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.RecordAsync(User.GetUserId(), dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] int? vendorId)
        => Ok(new { success = true, data = await _service.GetHistoryAsync(vendorId) });
}
