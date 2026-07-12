using System.Text;
using System.Text.Json;

namespace sabmero.Services;

public interface IEmailService
{
    Task<bool> SendAsync(string toEmail, string subject, string htmlBody);
    Task<bool> SendOtpAsync(string toEmail, string code);
}

// Sends emails through the Brevo (ex-Sendinblue) HTTP API.
//
// Why HTTP instead of SMTP: DigitalOcean blocks outbound SMTP ports
// (25/587/465) by default, so SmtpClient hangs forever on the droplet.
// Brevo's REST API runs over HTTPS/443, which is never blocked.
// Free tier: 300 emails/day, no credit card needed.
//
// Setup:
//  1. Sign up at https://www.brevo.com (free plan).
//  2. Verify the sender email (Senders & IP → Senders → Add a sender),
//     e.g. officialsabmero@gmail.com — click the confirmation mail.
//  3. Settings → SMTP & API → API Keys → Generate a new API key.
//  4. Config (appsettings.json or env vars Brevo__ApiKey etc.):
//       "Brevo": {
//         "ApiKey": "xkeysib-....",
//         "FromEmail": "officialsabmero@gmail.com",
//         "FromName": "SabMero"
//       }
//
// When no API key is configured, the mail body (including the OTP code)
// is written to the logs instead, so development keeps working.
public class EmailService : IEmailService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<EmailService> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody)
    {
        var apiKey = _config["Brevo:ApiKey"];
        var fromEmail = _config["Brevo:FromEmail"];
        var fromName = _config["Brevo:FromName"] ?? "SabMero";

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(fromEmail))
        {
            // Not configured — log the mail so development/testing still works.
            _logger.LogWarning("[EMAIL not configured] To={To} Subject={Subject} Body={Body}",
                toEmail, subject, htmlBody);
            return true;
        }

        try
        {
            var payload = new
            {
                sender = new { name = fromName, email = fromEmail },
                to = new[] { new { email = toEmail } },
                subject,
                htmlContent = htmlBody
            };

            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);   // never hang the request

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", apiKey);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent to {To}: {Subject}", toEmail, subject);
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Brevo send failed ({Status}): {Error}", response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            // Email failures must never break or hang the main request flow.
            _logger.LogError(ex, "Failed to send email to {To}", toEmail);
            return false;
        }
    }

    public Task<bool> SendOtpAsync(string toEmail, string code)
    {
        var body = $@"
            <div style=""font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:24px;border:1px solid #eee;border-radius:12px;"">
              <h2 style=""color:#0f766e;margin-top:0;"">SabMero Password Reset</h2>
              <p>Use the code below to reset your password. It is valid for <b>5 minutes</b>.</p>
              <div style=""font-size:32px;font-weight:bold;letter-spacing:8px;text-align:center;padding:16px;background:#f0fdfa;border-radius:8px;color:#0f766e;"">{code}</div>
              <p style=""color:#888;font-size:12px;"">If you didn't request this, you can safely ignore this email.</p>
            </div>";
        return SendAsync(toEmail, "Your SabMero password reset code", body);
    }
}