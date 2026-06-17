namespace sabmero.Services;

// Contract for sending notifications (SMS / WhatsApp / push).
// Kept generic so you can swap providers without touching callers.
public interface INotificationService
{
    Task SendSmsAsync(string phone, string message);
    Task SendOrderUpdateAsync(string phone, int orderId, string status);
    Task SendBookingUpdateAsync(string phone, int bookingId, string status);
}