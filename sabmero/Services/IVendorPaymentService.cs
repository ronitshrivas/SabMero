using sabmero.DTOs.Payment;

namespace sabmero.Services;

// Contract for the admin → vendor commission / settlement payment flow.
//
// Vendor side:
//   - Save / read their own payout QR.
//   - See the payments they've received + acknowledge receipt.
// Admin side:
//   - List vendors (with their QR + totals) to pay.
//   - Record a payment (amount + proof screenshot).
//   - See the full payment history (optionally filtered by vendor).
public interface IVendorPaymentService
{
    // ── Vendor ──
    Task<VendorQrDto?> GetMyQrAsync(int userId);
    Task<(bool Success, string Message)> SetMyQrAsync(int userId, string qrImagePath);
    Task<MyPayoutInfoDto?> GetMyPayoutsAsync(int userId);
    Task<(bool Success, string Message)> AcknowledgeAsync(int userId, int paymentId);

    // ── Admin ──
    Task<List<VendorPayoutSummaryDto>> GetVendorsForPayoutAsync();
    Task<(bool Success, string Message, VendorPaymentDto? Data)> RecordAsync(int adminUserId, RecordVendorPaymentDto dto);
    Task<List<VendorPaymentDto>> GetHistoryAsync(int? vendorId);
}
