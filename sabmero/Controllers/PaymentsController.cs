using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Payment;
using sabmero.Helpers;
using sabmero.Services;

namespace sabmero.Controllers;

// ── MANUAL QR PAYMENT ENDPOINTS ───────────────────────────────────────────────
//  GET    /api/payments/qr          → get the admin's QR to pay     (any logged-in)
//  PUT    /api/payments/qr          → set/replace the QR image      (Admin)
//  POST   /api/payments/submit      → submit a payment screenshot   (Customer)
//  POST   /api/payments/verify      → approve/reject a payment       (Admin/Vendor)
//  GET    /api/payments/pending     → payments awaiting verification (Admin/Vendor)
//
// Flow: admin uploads one QR image → customer scans & pays externally → customer
// uploads a screenshot (POST /api/uploads/payment) and submits it here → admin or
// vendor verifies it. A QR order/booking stays blocked until its payment is Verified.
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service)
    {
        _service = service;
    }

    // Any logged-in user can fetch the QR to pay.
    [HttpGet("qr")]
    public async Task<IActionResult> GetQr()
        => Ok(new { success = true, data = await _service.GetQrAsync() });

    // Admin sets/replaces the global payment QR.
    [Authorize(Roles = "Admin")]
    [HttpPut("qr")]
    public async Task<IActionResult> SetQr([FromBody] SetQrDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message) = await _service.SetQrAsync(dto.QrImagePath);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    // Customer submits their payment screenshot for an order or booking.
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitPaymentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message) = await _service.SubmitAsync(User.GetUserId(), dto);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    // Admin/Vendor approve or reject a submitted payment.
    [Authorize(Roles = "Admin,Vendor")]
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyPaymentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message) = await _service.VerifyAsync(User.GetUserId(), User.GetRole(), dto);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    // Admin/Vendor list payments awaiting verification.
    [Authorize(Roles = "Admin,Vendor")]
    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
        => Ok(new { success = true, data = await _service.GetPendingAsync(User.GetUserId(), User.GetRole()) });
}