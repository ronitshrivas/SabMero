using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Admin;

// Admin SENDS this to create a brand-new vendor from scratch: it makes both the
// User account and the approved Vendor profile in one step, and sets the role
// to Vendor. Unlike the request flow, no approval step is needed — the admin is
// creating an already-approved vendor directly.
public class CreateVendorAccountDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits")]
    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    // ── Vendor profile details ──
    [Required]
    [MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string BusinessAddress { get; set; } = string.Empty;

    public string? BusinessDocumentPath { get; set; }

    // Defaults to 10% if not supplied.
    public decimal? CommissionRate { get; set; }
}
