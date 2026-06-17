using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Promo;

// ── Admin SENDS this to create a promo code ──
public class CreatePromoDto
{
    [Required(ErrorMessage = "Code is required")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Discount must be between 1 and 100 percent")]
    public decimal DiscountPercent { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }
}

// ── Admin SENDS this to update a promo code ──
public class UpdatePromoDto
{
    [Range(1, 100)]
    public decimal DiscountPercent { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
}

// ── What the API SENDS BACK ──
public class PromoDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
}

// ── Customer SENDS this to validate a code before checkout ──
public class ValidatePromoDto
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Range(0, 100000000)]
    public decimal CartSubTotal { get; set; }
}

// ── Result of validating a promo ──
public class PromoValidationResult
{
    public bool Valid { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NewTotal { get; set; }
}