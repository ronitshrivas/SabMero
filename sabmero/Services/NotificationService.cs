namespace sabmero.Services;

// SAFE PLACEHOLDER notification service.
//
// Right now it just writes the message to the server logs (visible in Railway logs).
// When you're ready to send real messages, replace the body of SendSmsAsync with a
// call to your provider:
//   • SMS in Nepal: Sparrow SMS, Aakash SMS, etc. (HTTP POST with your API token)
//   • WhatsApp: Meta WhatsApp Cloud API or Twilio
// The two helper methods just format common messages and call SendSmsAsync, so you
// only have to wire up ONE method to go live.
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IConfiguration _config;

    public NotificationService(ILogger<NotificationService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public Task SendSmsAsync(string phone, string message)
    {
        // ── Replace this log line with a real HTTP call to your SMS provider ──
        // Example (pseudo):
        //   var token = _config["Sms:ApiToken"];
        //   await _http.PostAsync("https://api.sparrowsms.com/v2/sms/", ...);
        _logger.LogInformation("[SMS → {Phone}] {Message}", phone, message);
        return Task.CompletedTask;
    }

    public Task SendOrderUpdateAsync(string phone, int orderId, string status)
        => SendSmsAsync(phone, $"sabmero: Your order #{orderId} is now '{status}'.");

    public Task SendBookingUpdateAsync(string phone, int bookingId, string status)
        => SendSmsAsync(phone, $"sabmero: Your service booking #{bookingId} is now '{status}'.");
}