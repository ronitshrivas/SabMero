using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Review;

// ── Customer SENDS this to review a product OR a completed service ──
// Exactly one of ProductId / ServiceBookingId should be set.
public class CreateReviewDto
{
    public int? ProductId { get; set; }
    public int? ServiceBookingId { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be 1 to 5")]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;
}

// ── What the API SENDS BACK ──
public class ReviewDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public int? ProductId { get; set; }
    public int? ServiceBookingId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}