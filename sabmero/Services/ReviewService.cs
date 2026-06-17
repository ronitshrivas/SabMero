using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Review;
using sabmero.Models;

namespace sabmero.Services;

// Customers rate products they bought and services they received.
public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;

    public ReviewService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Success, string Message, ReviewDto? Data)> CreateAsync(int userId, CreateReviewDto dto)
    {
        // Exactly one target must be set.
        bool hasProduct = dto.ProductId.HasValue;
        bool hasBooking = dto.ServiceBookingId.HasValue;
        if (hasProduct == hasBooking)
            return (false, "Provide either a product or a service booking to review (not both).", null);

        if (hasProduct)
        {
            var product = await _db.Products.FindAsync(dto.ProductId!.Value);
            if (product == null)
                return (false, "Product not found.", null);

            // Customer must have actually bought it.
            bool purchased = await _db.OrderItems
                .AnyAsync(oi => oi.ProductId == dto.ProductId && oi.Order.UserId == userId);
            if (!purchased)
                return (false, "You can only review products you've purchased.", null);

            // One review per product per customer.
            bool already = await _db.Reviews
                .AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);
            if (already)
                return (false, "You've already reviewed this product.", null);
        }
        else
        {
            var booking = await _db.ServiceBookings.FindAsync(dto.ServiceBookingId!.Value);
            if (booking == null)
                return (false, "Booking not found.", null);
            if (booking.UserId != userId)
                return (false, "You can only review your own bookings.", null);
            if (booking.Status != "Completed")
                return (false, "You can only review completed services.", null);

            bool already = await _db.Reviews
                .AnyAsync(r => r.ServiceBookingId == dto.ServiceBookingId);
            if (already)
                return (false, "This service has already been reviewed.", null);
        }

        var review = new Review
        {
            UserId = userId,
            ProductId = dto.ProductId,
            ServiceBookingId = dto.ServiceBookingId,
            Rating = dto.Rating,
            Comment = dto.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        return (true, "Review submitted.", await BuildAsync(review.Id));
    }

    public async Task<List<ReviewDto>> GetForProductAsync(int productId)
        => await _db.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(MapExpr).ToListAsync();

    public async Task<List<ReviewDto>> GetForBookingAsync(int bookingId)
        => await _db.Reviews
            .Where(r => r.ServiceBookingId == bookingId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(MapExpr).ToListAsync();

    private async Task<ReviewDto?> BuildAsync(int id)
        => await _db.Reviews.Where(r => r.Id == id).Select(MapExpr).FirstOrDefaultAsync();

    private static readonly Expression<Func<Review, ReviewDto>> MapExpr = r => new ReviewDto
    {
        Id = r.Id,
        UserId = r.UserId,
        ReviewerName = r.User.FullName,
        ProductId = r.ProductId,
        ServiceBookingId = r.ServiceBookingId,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };
}