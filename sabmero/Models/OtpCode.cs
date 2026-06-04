namespace sabmero.Models;

// Stores 6-digit OTP codes sent to phones for login.
// Expires after 5 minutes. Marked IsUsed=true after verification.
public class OtpCode
{
    public int Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;     // 6-digit number as string
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}