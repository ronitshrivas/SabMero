namespace sabmero.Models;

// A customer's star rating + comment.
// Only ONE of ProductId OR ServiceBookingId will be set per row.
// Rating: 1 to 5 stars.
public class Review
{
    public int Id { get; set; }
    public int UserId { get; set; }             // FK → Users (who wrote the review)
    public int? ProductId { get; set; }         // set for product reviews
    public int? ServiceBookingId { get; set; }  // set for technician/service reviews
    public int Rating { get; set; }             // 1 to 5
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Product? Product { get; set; }
    public ServiceBooking? ServiceBooking { get; set; }
}