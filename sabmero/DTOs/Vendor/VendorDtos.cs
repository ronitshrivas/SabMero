using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Vendor;

// ── A logged-in user SENDS this to apply to become a vendor ──
// This creates a VendorRequest (Pending). The actual Vendor profile is only
// created once an admin approves the request.
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

// ── What the API SENDS BACK for a vendor profile (an approved vendor) ──
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

// ── What the API SENDS BACK for a vendor REQUEST ──
// Used by the status-check endpoint and the admin pending-requests list.
public class VendorRequestDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string? BusinessDocumentPath { get; set; }

    public string Status { get; set; } = string.Empty;     // "Pending" | "Approved" | "Rejected"
    public string? RejectionReason { get; set; }           // set only when Rejected
    public int? VendorId { get; set; }                     // the created Vendor profile (once Approved)

    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
