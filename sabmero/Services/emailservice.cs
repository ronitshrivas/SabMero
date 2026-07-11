using System.Net;
using System.Net.Mail;

namespace sabmero.Services;

public interface IEmailService
{
    Task<bool> SendAsync(string toEmail, string subject, string htmlBody);
    Task<bool> SendOtpAsync(string toEmail, string code);
}

// Sends emails over SMTP. Works with Gmail (use an App Password), Zoho,
// Brevo, Mailgun SMTP, etc. Configure in appsettings.json → "Smtp".
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody)
    {
        var host = _config["Smtp:Host"];
        var user = _config["Smtp:User"];
        var pass = _config["Smtp:Password"];
        var from = _config["Smtp:From"] ?? user;
        var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            // SMTP not configured yet — log the mail so development still works.
            _logger.LogWarning("[EMAIL not configured] To={To} Subject={Subject} Body={Body}", toEmail, subject, htmlBody);
            return true;
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(from!, "SabMero"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To}: {Subject}", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
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