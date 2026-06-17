using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Payment;

namespace sabmero.Services;

// Handles QR / online payments.
//
// IMPORTANT — this is a SAFE PLACEHOLDER, not a live payment integration.
// It generates a reference and a QR payload string your app can render as a QR code,
// then lets the order be marked "Paid" once confirmed. To go live in Nepal you would
// plug a real provider (FonePay / eSewa / Khalti) into InitiateAsync (build their QR
// payload) and ConfirmAsync (verify via their webhook/verification API) — the rest of
// the app already works against this interface and won't need to change.
public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(AppDbContext db, ILogger<PaymentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, PaymentInfoDto? Data)> InitiateAsync(int userId, InitiatePaymentDto dto)
    {
        var order = await _db.Orders.FindAsync(dto.OrderId);
        if (order == null)
            return (false, "Order not found.", null);
        if (order.UserId != userId)
            return (false, "You can only pay for your own orders.", null);
        if (order.PaymentStatus == "Paid")
            return (false, "This order is already paid.", null);

        // A unique reference the customer (and your records) can track this payment by.
        var reference = $"SAB-{order.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}";

        // Placeholder QR payload. Replace with the real PSP-generated payload when integrating.
        var qrPayload = $"sabmero://pay?ref={reference}&amount={order.TotalAmount}&currency=NPR";

        _logger.LogInformation("Payment initiated for order {OrderId}, ref {Reference}", order.Id, reference);

        return (true, "Scan the QR to pay.", new PaymentInfoDto
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            Currency = "NPR",
            QrPayload = qrPayload,
            Reference = reference,
            Instructions = "Open your mobile wallet, scan the QR, and confirm the payment."
        });
    }

    public async Task<(bool Success, string Message)> ConfirmAsync(int userId, string role, ConfirmPaymentDto dto)
    {
        var order = await _db.Orders.FindAsync(dto.OrderId);
        if (order == null)
            return (false, "Order not found.");

        // Customers can only confirm their own orders; Admin can confirm any.
        if (role == "Customer" && order.UserId != userId)
            return (false, "You can only confirm payment for your own orders.");

        if (order.PaymentStatus == "Paid")
            return (true, "Payment already confirmed.");

        // ── In PRODUCTION: verify dto.Reference against the PSP's verification API here ──
        // For now we trust the confirmation and mark the order paid.
        order.PaymentStatus = "Paid";
        if (order.PaymentMethod == "COD")
            order.PaymentMethod = "QR";   // they chose to pay online instead of cash

        await _db.SaveChangesAsync();
        _logger.LogInformation("Payment confirmed for order {OrderId}, ref {Reference}", dto.OrderId, dto.Reference);

        return (true, "Payment confirmed.");
    }
}