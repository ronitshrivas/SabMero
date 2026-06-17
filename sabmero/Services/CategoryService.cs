using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Category;
using sabmero.Models;

namespace sabmero.Services;

// Handles all category CRUD. Public read for everyone; create/update/delete are Admin-only
// (the controller enforces the [Authorize(Roles = "Admin")] part).
public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext db, ILogger<CategoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // List categories. Customers get only active ones; admin can ask for all.
    public async Task<List<CategoryDto>> GetAllAsync(bool includeInactive)
    {
        var query = _db.Categories.AsQueryable();
        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ImagePath = c.ImagePath,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        return await _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ImagePath = c.ImagePath,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(bool Success, string Message, CategoryDto? Data)> CreateAsync(CreateCategoryDto dto)
    {
        var name = dto.Name.Trim();

        // Prevent duplicate names (case-insensitive)
        bool exists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        if (exists)
            return (false, "A category with this name already exists.", null);

        var category = new Category
        {
            Name = name,
            ImagePath = dto.ImagePath,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Category created: {Name}", name);

        return (true, "Category created.", Map(category, 0));
    }

    public async Task<(bool Success, string Message, CategoryDto? Data)> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
            return (false, "Category not found.", null);

        category.Name = dto.Name.Trim();
        category.ImagePath = dto.ImagePath;
        category.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();

        var count = await _db.Products.CountAsync(p => p.CategoryId == id && p.IsActive);
        return (true, "Category updated.", Map(category, count));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
            return (false, "Category not found.");

        // Don't hard-delete a category that still has products — soft-disable instead.
        bool hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            category.IsActive = false;
            await _db.SaveChangesAsync();
            return (true, "Category has products, so it was deactivated instead of deleted.");
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return (true, "Category deleted.");
    }

    private static CategoryDto Map(Category c, int productCount) => new()
    {
        Id = c.Id,
        Name = c.Name,
        ImagePath = c.ImagePath,
        IsActive = c.IsActive,
        ProductCount = productCount
    };
}