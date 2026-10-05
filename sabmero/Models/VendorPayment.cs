namespace sabmero.Models;

// A commission / settlement payment the ADMIN makes to a VENDOR.
//
// Flow:
//   1. The vendor saves their payment QR on their profile (Vendor.PaymentQrPath).
//   2. The admin scans that QR, pays the vendor externally, and records the
//      payment here with the amount, an optional note, and a screenshot of the
//      transfer as proof.
//   3. The vendor sees the payment in their history and can mark it received
//      (acknowledge), giving a confirmation from both sides.
//
// Status values:  "Paid" (admin recorded it) → "Acknowledged" (vendor confirmed receipt)
public class VendorPayment
{
    public int Id { get; set; }

    public int VendorId { get; set; }                        // FK → Vendors
    public decimal Amount { get; set; }                      // amount paid to the vendor
    public string? Note { get; set; }                        // e.g. "Commission settlement — Sept 2026"
    public string? ScreenshotPath { get; set; }              // admin's proof-of-payment screenshot

    public string Status { get; set; } = "Paid";             // "Paid" | "Acknowledged"

    public int PaidByUserId { get; set; }                    // the admin who recorded it
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   // when the payment was recorded
    public DateTime? AcknowledgedAt { get; set; }            // when the vendor confirmed receipt

    // Navigation
    public Vendor Vendor { get; set; } = null!;
}
