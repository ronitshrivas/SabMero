using sabmero.DTOs.Review;

namespace sabmero.Services;

// Contract for review operations.
public interface IReviewService
{
    // Customer leaves a review for a product or a completed service booking.
    Task<(bool Success, string Message, ReviewDto? Data)> CreateAsync(int userId, CreateReviewDto dto);

    // Public: reviews for a product.
    Task<List<ReviewDto>> GetForProductAsync(int productId);

    // Public: reviews for a service booking (technician feedback).
    Task<List<ReviewDto>> GetForBookingAsync(int bookingId);
}