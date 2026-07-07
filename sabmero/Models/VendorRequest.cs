namespace sabmero.Models;

// A user's request to become a vendor.
// This is SEPARATE from the Vendor profile: the request is created on apply,
// and the actual Vendor profile + role upgrade only happen once an admin
// approves it. This lets us track Pending / Approved / Rejected with a reason
// without prematurely turning the user into a vendor.
//
// Status flow:
//   Pending  : submitted, waiting for admin review
//   Approved : admin approved → a Vendor profile is created, user role → Vendor
//   Rejected : admin declined → no profile created, RejectionReason is set
public class VendorRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }                          // FK → Users (the applicant)

    public string BusinessName { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string? BusinessDocumentPath { get; set; }        // uploaded registration/business doc
    public string? CitizenshipDocumentPath { get; set; }     // uploaded citizenship document
    public string? NidDocumentPath { get; set; }             // uploaded national ID (NID) card

    public string Status { get; set; } = "Pending";          // "Pending" | "Approved" | "Rejected"
    public string? RejectionReason { get; set; }             // set only when Status == "Rejected"

    // The Vendor profile created on approval (null while Pending/Rejected).
    public int? VendorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }                // when admin approved/rejected

    // Navigation
    public User User { get; set; } = null!;
    public Vendor? Vendor { get; set; }
}
