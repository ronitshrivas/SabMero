using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Payment;

// ── Customer asks the server for QR payment details for an order ──
public class InitiatePaymentDto
{
    [Required]
    public int OrderId { get; set; }
}

// ── What the API SENDS BACK: the QR string + amount to display ──
// In Nepal this is typically a FonePay/eSewa/Khalti QR payload.
// Until a real PSP is integrated, we return a placeholder payload.
public class PaymentInfoDto
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NPR";
    public string QrPayload { get; set; } = string.Empty;   // encode this into a QR in the app
    public string Reference { get; set; } = string.Empty;    // unique payment reference
    public string Instructions { get; set; } = string.Empty;
}

// ── Customer/PSP webhook confirms a payment was made ──
public class ConfirmPaymentDto
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public string Reference { get; set; } = string.Empty;
}