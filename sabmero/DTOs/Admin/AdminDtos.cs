using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Admin;

// ── Numbers shown on the Admin dashboard home screen ──
public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalVendors { get; set; }
    public int PendingVendors { get; set; }
    public int TotalTechnicians { get; set; }
    public int TotalRiders { get; set; }

    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }

    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int DeliveredOrders { get; set; }

    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }

    public decimal TotalSales { get; set; }        // sum of delivered order totals
    public decimal TotalCommission { get; set; }   // platform earnings
    public int PendingReturns { get; set; }
}

// ── Full user row for the admin user-management list ──
public class AdminUserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsKycVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Admin SENDS this to create a staff account (Technician/Rider) ──
public class CreateStaffDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    // "Technician" | "Rider"
    public string Role { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;
}

// ── Admin SENDS this to set a vendor's approval / commission ──
public class ApproveVendorDto
{
    public bool Approved { get; set; } = true;
    public decimal? CommissionRate { get; set; }   // optional override (e.g. 12.5)
}

// ── Admin SENDS this to toggle a user's active state ──
public class SetActiveDto
{
    public bool IsActive { get; set; }
}