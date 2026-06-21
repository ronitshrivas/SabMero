using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Service;
using sabmero.Models;

namespace sabmero.Services;

// Handles the on-site repair side of the app (Electrical, CCTV, Tech).
// Flow: Customer books → Admin assigns technician → Technician marks
//       Processing → OnTheWay → Completed (with final service charge).
public class ServiceBookingService : IServiceBookingService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ServiceBookingService> _logger;

    public ServiceBookingService(AppDbContext db, ILogger<ServiceBookingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, BookingDto? Data)> CreateAsync(int userId, CreateBookingDto dto)
    {
        var allowedTypes = new[] { "Electrical", "CCTV", "Tech" };
        if (!allowedTypes.Contains(dto.ServiceType))
            return (false, "Service type must be Electrical, CCTV, or Tech.", null);

        if (dto.BookingDate.Date < DateTime.UtcNow.Date)
            return (false, "Booking date cannot be in the past.", null);

        // Normalize the payment method up front.
        var paymentMethod = dto.PaymentMethod == "QR" ? "QR" : "Cash";

        // Cash → no payment step. QR → the customer pays the admin's QR and
        // submits a screenshot afterwards via POST /api/payments/submit, which
        // the admin then verifies. If a screenshot is already provided at
        // creation we accept it and mark the payment as Submitted right away.
        var hasShot = paymentMethod == "QR" && !string.IsNullOrWhiteSpace(dto.PaymentScreenshotPath);
        var paymentStatus = (paymentMethod == "QR" && hasShot) ? "Submitted" : "Pending";

        var booking = new ServiceBooking
        {
            UserId = userId,
            ServiceType = dto.ServiceType,
            BookingDate = DateTime.SpecifyKind(dto.BookingDate, DateTimeKind.Utc),
            TimeSlot = dto.TimeSlot.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ServiceAddress = dto.ServiceAddress.Trim(),
            DamageImagePath = dto.DamageImagePath,
            Status = "Pending",
            PaymentMethod = paymentMethod,
            PaymentScreenshotPath = paymentMethod == "QR" ? dto.PaymentScreenshotPath : null,
            PaymentStatus = paymentStatus,
            CreatedAt = DateTime.UtcNow
        };

        _db.ServiceBookings.Add(booking);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Service booking {Id} created by user {UserId}", booking.Id, userId);

        return (true, "Service booked successfully.", await BuildDtoAsync(booking.Id));
    }

    public async Task<List<BookingDto>> GetMyBookingsAsync(int userId)
    {
        var ids = await _db.ServiceBookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => b.Id).ToListAsync();
        return await BuildManyAsync(ids);
    }

    public async Task<List<BookingDto>> GetTechnicianBookingsAsync(int technicianId)
    {
        var ids = await _db.ServiceBookings
            .Where(b => b.TechnicianId == technicianId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => b.Id).ToListAsync();
        return await BuildManyAsync(ids);
    }

    public async Task<List<BookingDto>> GetAllAsync(string? status)
    {
        var q = _db.ServiceBookings.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(b => b.Status == status);

        var ids = await q.OrderByDescending(b => b.CreatedAt).Select(b => b.Id).ToListAsync();
        return await BuildManyAsync(ids);
    }

    public async Task<(bool Success, string Message, BookingDto? Data)> GetByIdAsync(int userId, string role, int bookingId)
    {
        var booking = await _db.ServiceBookings.FindAsync(bookingId);
        if (booking == null)
            return (false, "Booking not found.", null);

        // Customer sees own; Technician sees assigned; Admin sees all.
        if (role == "Customer" && booking.UserId != userId)
            return (false, "You can only view your own bookings.", null);
        if (role == "Technician" && booking.TechnicianId != userId)
            return (false, "This booking is not assigned to you.", null);

        var data = await BuildDtoAsync(bookingId);

        // While the booking is still under review (Pending / no technician yet),
        // tell the customer it's being reviewed instead of leaking a raw error
        // or empty technician details. Once approved, the technician's name and
        // phone come back in the data payload.
        var message = (booking.Status == "Pending" || booking.TechnicianId == null)
            ? "Your booking is currently under review. Once your status is approved, a technician will be assigned to you."
            : "Found.";

        return (true, message, data);
    }

    public async Task<(bool Success, string Message)> AssignTechnicianAsync(int bookingId, int technicianId)
    {
        var booking = await _db.ServiceBookings.FindAsync(bookingId);
        if (booking == null)
            return (false, "Booking not found.");

        var tech = await _db.Users.FindAsync(technicianId);
        if (tech == null || tech.Role != "Technician")
            return (false, "That user is not a technician.");

        // A QR booking can't be approved/assigned until its payment is verified.
        if (booking.PaymentMethod == "QR" && booking.PaymentStatus != "Verified")
            return (false, "Payment is not verified yet. Verify the payment screenshot before assigning a technician.");

        booking.TechnicianId = technicianId;
        // Assigning a technician approves the booking and reveals the
        // technician's details to the customer.
        if (booking.Status == "Pending")
            booking.Status = "Approved";

        await _db.SaveChangesAsync();
        return (true, "Technician assigned.");
    }

    public async Task<(bool Success, string Message)> UpdateStatusAsync(int userId, string role, int bookingId, UpdateBookingStatusDto dto)
    {
        var allowed = new[] { "Pending", "Approved", "Processing", "OnTheWay", "Completed" };
        if (!allowed.Contains(dto.Status))
            return (false, "Invalid status.");

        var booking = await _db.ServiceBookings.FindAsync(bookingId);
        if (booking == null)
            return (false, "Booking not found.");

        // Technician can only update bookings assigned to them.
        if (role == "Technician" && booking.TechnicianId != userId)
            return (false, "This booking is not assigned to you.");

        // A booking can only be Approved once a technician has been assigned,
        // because approval is what surfaces the technician's details.
        if (dto.Status == "Approved" && booking.TechnicianId == null)
            return (false, "Assign a technician before approving this booking.");

        // A QR booking can't be approved until its payment is verified.
        if (dto.Status == "Approved" && booking.PaymentMethod == "QR" && booking.PaymentStatus != "Verified")
            return (false, "Payment is not verified yet. Verify the payment screenshot before approving this booking.");

        booking.Status = dto.Status;

        // Timestamps for the technician workflow.
        if (dto.Status == "OnTheWay" && booking.CheckInTime == null)
            booking.CheckInTime = DateTime.UtcNow;

        if (dto.Status == "Completed")
        {
            booking.CompletedTime = DateTime.UtcNow;
            if (dto.ServiceCharge.HasValue)
                booking.ServiceCharge = dto.ServiceCharge.Value;
        }

        await _db.SaveChangesAsync();
        return (true, $"Booking status updated to {dto.Status}.");
    }

    public async Task<(bool Success, string Message)> CancelAsync(int userId, int bookingId)
    {
        var booking = await _db.ServiceBookings.FindAsync(bookingId);
        if (booking == null)
            return (false, "Booking not found.");
        if (booking.UserId != userId)
            return (false, "You can only cancel your own bookings.");
        if (booking.Status != "Pending" && booking.Status != "Processing")
            return (false, "This booking can no longer be cancelled.");

        // Delete the booking record on cancellation.
        _db.ServiceBookings.Remove(booking);
        await _db.SaveChangesAsync();
        return (true, "Booking cancelled.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private async Task<List<BookingDto>> BuildManyAsync(List<int> ids)
    {
        var result = new List<BookingDto>();
        foreach (var id in ids)
        {
            var dto = await BuildDtoAsync(id);
            if (dto != null) result.Add(dto);
        }
        return result;
    }

    private async Task<BookingDto?> BuildDtoAsync(int bookingId)
    {
        var b = await _db.ServiceBookings
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == bookingId);
        if (b == null) return null;

        string? techName = null;
        string? techPhone = null;
        // Only reveal the technician's details once the booking is approved
        // (i.e. a technician has actually been assigned to this customer).
        if (b.TechnicianId.HasValue && b.Status != "Pending")
        {
            var tech = await _db.Users.FindAsync(b.TechnicianId.Value);
            techName = tech?.FullName;
            techPhone = tech?.Phone;
        }

        return new BookingDto
        {
            Id = b.Id,
            UserId = b.UserId,
            CustomerName = b.User.FullName,
            CustomerPhone = b.User.Phone,
            TechnicianId = b.TechnicianId,
            TechnicianName = techName,
            TechnicianPhone = techPhone,
            ServiceType = b.ServiceType,
            BookingDate = b.BookingDate,
            TimeSlot = b.TimeSlot,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            ServiceAddress = b.ServiceAddress,
            DamageImagePath = b.DamageImagePath,
            Status = b.Status,
            CheckInTime = b.CheckInTime,
            CompletedTime = b.CompletedTime,
            PaymentMethod = b.PaymentMethod,
            PaymentScreenshotPath = b.PaymentScreenshotPath,
            PaymentStatus = b.PaymentStatus,
            ServiceCharge = b.ServiceCharge,
            CreatedAt = b.CreatedAt
        };
    }
}