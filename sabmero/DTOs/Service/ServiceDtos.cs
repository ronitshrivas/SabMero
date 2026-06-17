using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Service;

// ── Customer SENDS this to book an on-site repair ──
public class CreateBookingDto
{
    [Required(ErrorMessage = "Service type is required")]
    // "Electrical" | "CCTV" | "Tech"
    public string ServiceType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Booking date is required")]
    public DateTime BookingDate { get; set; }

    [Required(ErrorMessage = "Time slot is required")]
    public string TimeSlot { get; set; } = string.Empty;   // e.g. "10:00 AM - 12:00 PM"

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [Required(ErrorMessage = "Service address is required")]
    [MaxLength(400)]
    public string ServiceAddress { get; set; } = string.Empty;

    public string? DamageImagePath { get; set; }   // optional photo of the problem

    // "Cash" | "QR"
    public string PaymentMethod { get; set; } = "Cash";
}

// ── What the API SENDS BACK for a booking ──
public class BookingDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string ServiceAddress { get; set; } = string.Empty;
    public string? DamageImagePath { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CheckInTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal? ServiceCharge { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Admin SENDS this to assign a technician ──
public class AssignTechnicianDto
{
    [Required]
    public int TechnicianId { get; set; }
}

// ── Technician/Admin SENDS this to update a booking's status ──
public class UpdateBookingStatusDto
{
    [Required]
    // "Pending" | "Processing" | "OnTheWay" | "Completed"
    public string Status { get; set; } = string.Empty;

    // Set when the technician completes the job and enters the final charge.
    public decimal? ServiceCharge { get; set; }
}