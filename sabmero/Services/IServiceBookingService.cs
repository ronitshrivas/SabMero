using sabmero.DTOs.Service;

namespace sabmero.Services;

// Contract for on-site repair/service booking operations.
public interface IServiceBookingService
{
    // Customer creates a booking.
    Task<(bool Success, string Message, BookingDto? Data)> CreateAsync(int userId, CreateBookingDto dto);

    // Customer: my bookings.
    Task<List<BookingDto>> GetMyBookingsAsync(int userId);

    // Technician: bookings assigned to me.
    Task<List<BookingDto>> GetTechnicianBookingsAsync(int technicianId);

    // Admin: all bookings (optionally filtered by status).
    Task<List<BookingDto>> GetAllAsync(string? status);

    // Get one booking with ownership/role check.
    Task<(bool Success, string Message, BookingDto? Data)> GetByIdAsync(int userId, string role, int bookingId);

    // Admin: assign a technician.
    Task<(bool Success, string Message)> AssignTechnicianAsync(int bookingId, int technicianId);

    // Technician/Admin: update status + optional service charge.
    Task<(bool Success, string Message)> UpdateStatusAsync(int userId, string role, int bookingId, UpdateBookingStatusDto dto);

    // Customer: cancel my own booking while still Pending/Processing.
    Task<(bool Success, string Message)> CancelAsync(int userId, int bookingId);
}