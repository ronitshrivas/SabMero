using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Auth;

// ── What the API SENDS BACK for the current user's KYC status ──
// Returned by GET /api/Auth/kyc-status so the app can show whether the user's
// KYC is NotSubmitted / Pending / Approved / Rejected (with a reason).
public class KycStatusDto
{
    public string Status { get; set; } = "NotSubmitted";   // "NotSubmitted" | "Pending" | "Approved" | "Rejected"
    public string? RejectionReason { get; set; }           // set only when Status == "Rejected"
    public string? DocumentPath { get; set; }              // the uploaded document, if any
    public bool IsVerified { get; set; }                   // convenience: Status == "Approved"
}

// ── A logged-in user SENDS this to submit (or re-submit) their KYC ──
// Upload the file via POST /api/Uploads/kyc first, then send the returned path.
public class SubmitKycDto
{
    [Required(ErrorMessage = "KYC document is required")]
    public string DocumentPath { get; set; } = string.Empty;
}
