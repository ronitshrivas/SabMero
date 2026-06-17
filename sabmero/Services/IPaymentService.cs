using sabmero.DTOs.Payment;

namespace sabmero.Services;

// Contract for QR/online payment operations.
public interface IPaymentService
{
    // Customer requests payment info (QR payload) for an order they own.
    Task<(bool Success, string Message, PaymentInfoDto? Data)> InitiateAsync(int userId, InitiatePaymentDto dto);

    // Confirm a payment was completed (called by the app after scan, or by a PSP webhook).
    Task<(bool Success, string Message)> ConfirmAsync(int userId, string role, ConfirmPaymentDto dto);
}