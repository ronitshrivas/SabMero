namespace sabmero.Models;

// Simple key-value store for global app settings the admin controls.
// Currently used to hold the admin's payment QR image path under the
// well-known key "PaymentQrImagePath".
public class AppSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;     // unique setting name
    public string? Value { get; set; }                  // setting value (e.g. an image path)
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}