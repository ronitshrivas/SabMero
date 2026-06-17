using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Product;
using sabmero.Helpers;
using sabmero.Services;

namespace sabmero.Controllers;

// ── PRODUCT ENDPOINTS ─────────────────────────────────────────────────────────
//  GET    /api/products                  → browse catalogue (search/filter/page)  (public)
//  GET    /api/products/{id}             → product detail                          (public)
//  GET    /api/products/mine             → my products                             (Vendor)
//  POST   /api/products                  → create product                          (Vendor)
//  PUT    /api/products/{id}             → update product                          (Vendor/Admin)
//  DELETE /api/products/{id}             → remove product                          (Vendor/Admin)
//
// Browse query params (all optional):
//  ?search=shoes&categoryId=3&vendorId=2&minPrice=100&maxPrice=5000
//  &sortBy=price_asc|price_desc|rating|newest&page=1&pageSize=20
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Browse(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? vendorId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var data = await _service.BrowseAsync(search, categoryId, vendorId, minPrice, maxPrice, sortBy, page, pageSize);
        return Ok(new { success = true, data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data == null)
            return NotFound(new { success = false, message = "Product not found." });
        return Ok(new { success = true, data });
    }

    [Authorize(Roles = "Vendor,Admin")]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var data = await _service.GetByVendorUserAsync(User.GetUserId());
        return Ok(new { success = true, data });
    }

    [Authorize(Roles = "Vendor,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.CreateAsync(User.GetUserId(), dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [Authorize(Roles = "Vendor,Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message, data) = await _service.UpdateAsync(User.GetUserId(), User.GetRole(), id, dto);
        return success
            ? Ok(new { success = true, message, data })
            : BadRequest(new { success = false, message });
    }

    [Authorize(Roles = "Vendor,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _service.DeleteAsync(User.GetUserId(), User.GetRole(), id);
        return success
            ? Ok(new { success = true, message })
            : BadRequest(new { success = false, message });
    }
}