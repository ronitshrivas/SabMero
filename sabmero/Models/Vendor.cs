using sabmero.Models;

namespace sabmero.Models;

// Each Vendor is linked to one User account.
// Admin must set IsApproved = true before the vendor can sell.
public class Vendor
{
    public int Id { get; set; }
    public int UserId { get; set; }                          // FK → Users table
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string? BusinessDocumentPath { get; set; }        // uploaded KYC doc
    public bool IsApproved { get; set; } = false;
    public decimal CommissionRate { get; set; } = 10.0m;     // 10% default
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}