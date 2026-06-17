using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Vendor;

// ── A logged-in user SENDS this to apply to become a vendor ──
// (They must already have a User account; this creates their Vendor profile.)
public class CreateVendorDto
{
    [Required(ErrorMessage = "Business name is required")]
    [MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Business address is required")]
    [MaxLength(300)]
    public string BusinessAddress { get; set; } = string.Empty;

    public string? BusinessDocumentPath { get; set; }   // uploaded registration/KYC doc
}

// ── What the API SENDS BACK for a vendor profile ──
public class VendorDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;   // the User's full name
    public string Phone { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string? BusinessDocumentPath { get; set; }
    public bool IsApproved { get; set; }
    public decimal CommissionRate { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
}