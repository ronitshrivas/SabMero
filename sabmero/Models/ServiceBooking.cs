using sabmero.Models;

namespace sabmero.Models;

// A customer's booking for an on-site repair technician.
// ServiceType: "Electrical" | "CCTV" | "Tech"
// Status flow:  Pending → Processing → OnTheWay → Completed
public class ServiceBooking
{
    public int Id { get; set; }
    public int UserId { get; set; }             // FK → Users (the customer)
    public int? TechnicianId { get; set; }      // FK → Users (assigned later by admin)
    public string ServiceType { get; set; } = string.Empty;  // "Electrical" | "CCTV" | "Tech"
    public DateTime BookingDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;     // e.g. "10:00 AM - 12:00 PM"
    public double Latitude { get; set; }        // from Google Maps
    public double Longitude { get; set; }
    public string ServiceAddress { get; set; } = string.Empty;
    public string? DamageImagePath { get; set; }             // photo uploaded by customer
    public string Status { get; set; } = "Pending";          // "Pending"|"Processing"|"OnTheWay"|"Completed"
    public DateTime? CheckInTime { get; set; }               // technician taps "Check-In"
    public DateTime? CompletedTime { get; set; }             // technician taps "Complete"
    public string PaymentMethod { get; set; } = "Cash";      // "Cash" | "QR"
    public decimal? ServiceCharge { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Review? Review { get; set; }
}