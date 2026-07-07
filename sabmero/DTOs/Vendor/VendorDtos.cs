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

    public string? BusinessDocumentPath { get; set; }      // uploaded registration/business doc
    public string? CitizenshipDocumentPath { get; set; }   // uploaded citizenship document
    public string? NidDocumentPath { get; set; }           // uploaded national ID (NID) card
}

// ── A NEW (not-yet-registered) person SENDS this to sign up as a vendor ──
// This is PUBLIC (no login). It creates the User account AND a Pending
// VendorRequest in one step, with all three documents. The Vendor profile and
// role upgrade still only happen once an admin approves the request.
public class RegisterVendorDto
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits")]
    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Business name is required")]
    [MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Business address is required")]
    [MaxLength(300)]
    public string BusinessAddress { get; set; } = string.Empty;

    // All three documents are required for self-registration.
    [Required(ErrorMessage = "Citizenship document is required")]
    public string CitizenshipDocumentPath { get; set; } = string.Empty;

    [Required(ErrorMessage = "NID document is required")]
    public string NidDocumentPath { get; set; } = string.Empty;

    [Required(ErrorMessage = "Business document is required")]
    public string BusinessDocumentPath { get; set; } = string.Empty;
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
    public string? CitizenshipDocumentPath { get; set; }
    public string? NidDocumentPath { get; set; }

    public string Status { get; set; } = string.Empty;     // "Pending" | "Approved" | "Rejected"
    public string? RejectionReason { get; set; }           // set only when Rejected
    public int? VendorId { get; set; }                     // the created Vendor profile (once Approved)

    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}