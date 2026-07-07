using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.Services;

namespace sabmero.Controllers;

// ── FILE UPLOAD ENDPOINTS ─────────────────────────────────────────────────────
//  POST   /api/uploads/product   → upload a product image     (Vendor/Admin)
//  POST   /api/uploads/damage    → upload a damage photo       (Customer)
//  POST   /api/uploads/payment   → upload a QR payment proof    (Customer)
//  POST   /api/uploads/kyc       → upload a KYC / business doc  (any logged-in)
//
// Send as multipart/form-data with a single field named "file".
// Returns { path: "/uploads/<folder>/<filename>" } — save that path on the
// related record (e.g. product.ImagePath, booking.DamageImagePath).
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadsController : ControllerBase
{
    private readonly IFileService _files;

    public UploadsController(IFileService files)
    {
        _files = files;
    }

    [Authorize(Roles = "Vendor,Admin")]
    [HttpPost("product")]
    public Task<IActionResult> Product(IFormFile file) => Handle(file, "products");

    [HttpPost("damage")]
    public Task<IActionResult> Damage(IFormFile file) => Handle(file, "damage");

    // QR payment proof — the customer uploads here, then sends the returned
    // path as CreateBookingDto.PaymentScreenshotPath when booking with QR.
    [HttpPost("payment")]
    public Task<IActionResult> Payment(IFormFile file) => Handle(file, "payment");

    [AllowAnonymous]
    [HttpPost("kyc")]
    public Task<IActionResult> Kyc(IFormFile file) => Handle(file, "kyc");

    private async Task<IActionResult> Handle(IFormFile file, string folder)
    {
        var (success, message, path) = await _files.SaveAsync(file, folder);
        return success
            ? Ok(new { success = true, message, data = new { path } })
            : BadRequest(new { success = false, message });
    }
}