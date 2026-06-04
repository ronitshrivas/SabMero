using sabmero.Models;

namespace sabmero.Models;

// Product categories — Electronics, Clothing, Footwear, Grocery etc.
// Admin can add/edit/delete categories dynamically from the Admin Panel.
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}