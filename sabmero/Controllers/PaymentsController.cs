using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Payment;
using sabmero.Helpers;
using sabmero.Services;

namespace sabmero.Controllers;

// ── PAYMENT ENDPOINTS ─────────────────────────────────────────────────────────
//  POST   /api/payments/initiate    → get QR payload for an order   (Customer)
//  POST   /api/payments/confirm     → confirm payment completed     (Customer/Admin)
//
// NOTE: This is a placeholder QR flow. See PaymentService for how to plug in a real
// Nepali payment provider (FonePay / eSewa / Khalti) later.
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

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.InitiateAsync(User.GetUserId(), dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message) = await _service.ConfirmAsync(User.GetUserId(), User.GetRole(), dto);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }
}