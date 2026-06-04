namespace sabmero.Models;

// Discount codes managed by Admin.
// Customers type the Code at checkout to get DiscountPercent % off.
public class PromoCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;        // e.g. "SAVE10"
    public decimal DiscountPercent { get; set; }            // e.g. 10 = 10% off
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}