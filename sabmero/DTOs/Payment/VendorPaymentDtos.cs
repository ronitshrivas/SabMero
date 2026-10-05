using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Payment;

// ── VENDOR sets/replaces their own payout QR ──
// Upload the QR image first via POST /api/uploads/payment, then send the path here.
public class SetVendorQrDto
{
    [Required(ErrorMessage = "QR image path is required")]
    [MaxLength(400)]
    public string QrImagePath { get; set; } = string.Empty;
}

// ── What the vendor gets back when reading their own QR ──
public class VendorQrDto
{
    public string? QrImagePath { get; set; }   // null until the vendor uploads one
}

// ── ADMIN records a commission / settlement payment to a vendor ──
// Upload the proof screenshot first via POST /api/uploads/payment, then send
// its path here along with the amount.
public class RecordVendorPaymentDto
{
    [Required]
    public int VendorId { get; set; }

    [Required]
    [Range(0.01, 100000000, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [MaxLength(300)]
    public string? Note { get; set; }

    [Required(ErrorMessage = "A payment screenshot is required.")]
    [MaxLength(400)]
    public string ScreenshotPath { get; set; } = string.Empty;
}

// ── One commission payment row (shown to both admin and vendor) ──
public class VendorPaymentDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;   // business name
    public string OwnerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public string? ScreenshotPath { get; set; }
    public string Status { get; set; } = "Paid";             // "Paid" | "Acknowledged"
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}

// ── A vendor row in the admin "pay your vendors" list ──
public class VendorPayoutSummaryDto
{
    public int VendorId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PaymentQrPath { get; set; }               // the vendor's QR (null if not set)
    public decimal CommissionRate { get; set; }
    public decimal TotalPaid { get; set; }                   // sum of all payments made so far
    public DateTime? LastPaidAt { get; set; }
}

// ── What the vendor sees on their payouts page ──
public class MyPayoutInfoDto
{
    public string? PaymentQrPath { get; set; }
    public decimal TotalReceived { get; set; }
    public List<VendorPaymentDto> Payments { get; set; } = new();
}
