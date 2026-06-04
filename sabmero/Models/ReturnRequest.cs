namespace sabmero.Models;

// A customer's request to return/refund an order.
// Admin reviews from the Admin Panel and sets Status to Approved or Rejected.
public class ReturnRequest
{
    public int Id { get; set; }
    public int OrderId { get; set; }        // FK → Orders (one-to-one)
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";     // "Pending" | "Approved" | "Rejected"
    public string? AdminNote { get; set; }               // admin's reply note
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Order Order { get; set; } = null!;
}