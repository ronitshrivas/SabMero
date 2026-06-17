using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Category;

// ── What Flutter SENDS to create a category (Admin only) ──
public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? ImagePath { get; set; }   // optional image URL/path
}

// ── What Flutter SENDS to update a category (Admin only) ──
public class UpdateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? ImagePath { get; set; }
    public bool IsActive { get; set; } = true;
}

// ── What the API SENDS BACK for any category ──
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }   // how many active products in this category
}