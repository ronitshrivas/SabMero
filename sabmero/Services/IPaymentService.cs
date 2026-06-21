using sabmero.DTOs.Payment;

namespace sabmero.Services;

// Contract for the manual (admin-managed) QR payment flow.
//
// Flow:
//   1. Admin uploads a single global QR image       → SetQrAsync
//   2. Any user fetches it to pay                    → GetQrAsync
//   3. Customer pays externally, uploads screenshot  → SubmitAsync
//   4. Admin / Vendor approve or reject the proof    → VerifyAsync
//   5. Admin / Vendor see what's awaiting them        → GetPendingAsync
public interface IPaymentService
{
    // Admin: set the global payment QR image path.
    Task<(bool Success, string Message)> SetQrAsync(string qrImagePath);

    // Any logged-in user: get the current QR to display at checkout.
    Task<QrInfoDto> GetQrAsync();

    // Customer: submit a payment screenshot for an Order or Booking they own.
    Task<(bool Success, string Message)> SubmitAsync(int userId, SubmitPaymentDto dto);

    // Admin/Vendor: approve or reject a submitted payment.
    Task<(bool Success, string Message)> VerifyAsync(int userId, string role, VerifyPaymentDto dto);

    // Admin/Vendor: list payments awaiting verification.
    //  - Admin sees all submitted payments (orders + bookings).
    //  - Vendor sees only submitted-payment orders that contain their products.
    Task<List<PendingPaymentDto>> GetPendingAsync(int userId, string role);
}