using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Promo;
using sabmero.Services;

namespace sabmero.Controllers;

// ── PROMO CODE ENDPOINTS ──────────────────────────────────────────────────────
//  POST   /api/promos/validate     → check a code against a cart subtotal   (Customer)
//  GET    /api/promos              → list all promo codes                   (Admin)
//  POST   /api/promos              → create a promo code                    (Admin)
//  PUT    /api/promos/{id}         → update a promo code                    (Admin)
//  DELETE /api/promos/{id}         → delete a promo code                    (Admin)
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
public class PromosController : ControllerBase
{
    private readonly IPromoService _service;

    public PromosController(IPromoService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidatePromoDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.ValidateAsync(dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new { success = true, data = await _service.GetAllAsync() });

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromoDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.CreateAsync(dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePromoDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.UpdateAsync(id, dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }
}