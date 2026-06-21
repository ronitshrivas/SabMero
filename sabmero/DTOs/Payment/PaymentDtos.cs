using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Payment;

// ── Admin SENDS this to set the global payment QR image ──
// Upload the QR image first via POST /api/uploads/payment, then send the
// returned path here.
public class SetQrDto
{
    [Required(ErrorMessage = "QR image path is required")]
    [MaxLength(400)]
    public string QrImagePath { get; set; } = string.Empty;
}

// ── What the API SENDS BACK when a user asks for the QR to pay ──
public class QrInfoDto
{
    public string? QrImagePath { get; set; }   // the admin's uploaded QR image (null if not set yet)
    public string Instructions { get; set; } =
        "Scan this QR with your payment app, pay the amount, then upload a screenshot of the payment.";
}

// ── Customer SENDS this to submit their payment screenshot ──
// Upload the screenshot first via POST /api/uploads/payment, then send the
// returned path here together with what it's paying for.
public class SubmitPaymentDto
{
    [Required]
    // "Order" | "Booking"
    public string Type { get; set; } = string.Empty;

    [Required]
    public int Id { get; set; }     // the Order id or ServiceBooking id

    [Required(ErrorMessage = "Payment screenshot is required")]
    [MaxLength(400)]
    public string ScreenshotPath { get; set; } = string.Empty;
}

// ── Admin/Vendor SENDS this to verify (approve/reject) a submitted payment ──
public class VerifyPaymentDto
{
    [Required]
    // "Order" | "Booking"
    public string Type { get; set; } = string.Empty;

    [Required]
    public int Id { get; set; }

    [Required]
    public bool Approve { get; set; }    // true = Verified, false = Rejected

    [MaxLength(300)]
    public string? Note { get; set; }    // optional reason, mainly for rejections
}

// ── A row in the "payments awaiting verification" list ──
public class PendingPaymentDto
{
    public string Type { get; set; } = string.Empty;   // "Order" | "Booking"
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? ScreenshotPath { get; set; }
    public DateTime CreatedAt { get; set; }
}