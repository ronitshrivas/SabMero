using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Service;
using sabmero.Helpers;
using sabmero.Services;

namespace sabmero.Controllers;

// ── SERVICE BOOKING ENDPOINTS ─────────────────────────────────────────────────
//  POST   /api/bookings                 → book a repair service      (Customer)
//  GET    /api/bookings/mine            → my bookings                (Customer)
//  GET    /api/bookings/{id}            → one booking                (owner/tech/Admin)
//  POST   /api/bookings/{id}/cancel     → cancel my booking          (Customer)
//  GET    /api/bookings/technician      → bookings assigned to me    (Technician)
//  PUT    /api/bookings/{id}/status     → update status + charge     (Technician/Admin)
//
// (Admin listing & technician assignment live under /api/admin/bookings.)
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IServiceBookingService _service;

    public BookingsController(IServiceBookingService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.CreateAsync(User.GetUserId(), dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var data = await _service.GetMyBookingsAsync(User.GetUserId());
        return Ok(new { success = true, data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var (success, message, data) = await _service.GetByIdAsync(User.GetUserId(), User.GetRole(), id);
        return success
            ? Ok(new { success = true, message, data })
            : NotFound(new { success = false, message });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, message) = await _service.CancelAsync(User.GetUserId(), id);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }

    [Authorize(Roles = "Technician")]
    [HttpGet("technician")]
    public async Task<IActionResult> TechnicianBookings()
    {
        var data = await _service.GetTechnicianBookingsAsync(User.GetUserId());
        return Ok(new { success = true, data });
    }

    [Authorize(Roles = "Technician,Admin")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBookingStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message) = await _service.UpdateStatusAsync(User.GetUserId(), User.GetRole(), id, dto);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }
}