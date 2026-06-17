using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Category;
using sabmero.Services;

namespace sabmero.Controllers;

// ── CATEGORY ENDPOINTS ────────────────────────────────────────────────────────
//  GET    /api/categories            → list active categories      (public)
//  GET    /api/categories/{id}       → one category                (public)
//  POST   /api/categories            → create category             (Admin)
//  PUT    /api/categories/{id}       → update category             (Admin)
//  DELETE /api/categories/{id}       → delete/deactivate category  (Admin)
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    // Public — anyone can see categories. ?includeInactive=true is honoured for admins.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var data = await _service.GetAllAsync(includeInactive);
        return Ok(new { success = true, data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data == null)
            return NotFound(new { success = false, message = "Category not found." });
        return Ok(new { success = true, data });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.CreateAsync(dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
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