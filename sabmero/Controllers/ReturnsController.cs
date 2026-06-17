using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Return;
using sabmero.Helpers;
using sabmero.Services;

namespace sabmero.Controllers;

// ── RETURN / REFUND ENDPOINTS ─────────────────────────────────────────────────
//  POST   /api/returns                 → request a return            (Customer)
//  GET    /api/returns/mine            → my return requests          (Customer)
//  GET    /api/returns                 → all return requests         (Admin)
//  PUT    /api/returns/{id}/resolve    → approve / reject            (Admin)
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReturnsController : ControllerBase
{
    private readonly IReturnService _service;

    public ReturnsController(IReturnService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReturnDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.CreateAsync(User.GetUserId(), dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
        => Ok(new { success = true, data = await _service.GetMyReturnsAsync(User.GetUserId()) });

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
        => Ok(new { success = true, data = await _service.GetAllAsync(status) });

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/resolve")]
    public async Task<IActionResult> Resolve(int id, [FromBody] ResolveReturnDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message) = await _service.ResolveAsync(id, dto);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }
}