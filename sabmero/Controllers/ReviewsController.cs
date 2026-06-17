using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Review;
using sabmero.Helpers;
using sabmero.Services;

namespace sabmero.Controllers;

// ── REVIEW ENDPOINTS ──────────────────────────────────────────────────────────
//  POST   /api/reviews                       → leave a review              (Customer)
//  GET    /api/reviews/product/{productId}   → reviews for a product       (public)
//  GET    /api/reviews/booking/{bookingId}   → reviews for a service       (public)
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewsController(IReviewService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.CreateAsync(User.GetUserId(), dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> ForProduct(int productId)
        => Ok(new { success = true, data = await _service.GetForProductAsync(productId) });

    [HttpGet("booking/{bookingId:int}")]
    public async Task<IActionResult> ForBooking(int bookingId)
        => Ok(new { success = true, data = await _service.GetForBookingAsync(bookingId) });
}