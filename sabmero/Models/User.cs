using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Numerics;

namespace sabmero.Models;

// This represents the "Users" table in PostgreSQL.
// One row = one person (Customer, Vendor, Admin, Technician, or Rider).
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;   // used as login ID
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // Possible values: "Customer" | "Vendor" | "Admin" | "Technician" | "Rider"
    public string Role { get; set; } = "Customer";

    public bool IsKycVerified { get; set; } = false;
    public string? KycDocumentPath { get; set; }        // file path on server
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties (EF Core uses these to JOIN tables)
    public Vendor? Vendor { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<ServiceBooking> ServiceBookings { get; set; } = new List<ServiceBooking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}