using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using sabmero.Models;

namespace sabmero.Data;

// This is the "bridge" between your C# code and the PostgreSQL database.
// Every DbSet<X> below becomes a table in PostgreSQL.
// OnModelCreating() defines table rules (primary keys, foreign keys, column sizes).
// SeedData() inserts default rows when the database is first created.

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Each line below = one table in PostgreSQL ──────────────────────────
    public DbSet<User> Users { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ServiceBooking> ServiceBookings { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }
    public DbSet<ReturnRequest> ReturnRequests { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── USERS ──────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Phone).IsUnique();          // no two users with same phone
            e.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Phone).HasMaxLength(20).IsRequired();
            e.Property(u => u.Email).HasMaxLength(150);
            e.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("Customer");
            e.Property(u => u.Address).HasMaxLength(300);
        });

        // ── OTP CODES ──────────────────────────────────────────────────────
        modelBuilder.Entity<OtpCode>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Phone).HasMaxLength(20).IsRequired();
            e.Property(o => o.Code).HasMaxLength(6).IsRequired();
        });

        // ── VENDORS ────────────────────────────────────────────────────────
        modelBuilder.Entity<Vendor>(e =>
        {
            e.HasKey(v => v.Id);

            // One User → One Vendor (one-to-one)
            e.HasOne(v => v.User)
             .WithOne(u => u.Vendor)
             .HasForeignKey<Vendor>(v => v.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(v => v.BusinessName).HasMaxLength(200).IsRequired();
            e.Property(v => v.BusinessAddress).HasMaxLength(300);
            e.Property(v => v.CommissionRate).HasPrecision(5, 2);
        });

        // ── CATEGORIES ─────────────────────────────────────────────────────
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
        });

        // ── PRODUCTS ───────────────────────────────────────────────────────
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);

            // One Vendor → Many Products
            e.HasOne(p => p.Vendor)
             .WithMany(v => v.Products)
             .HasForeignKey(p => p.VendorId)
             .OnDelete(DeleteBehavior.Cascade);

            // One Category → Many Products
            e.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);   // can't delete category if products exist

            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.Property(p => p.Unit).HasMaxLength(10);
        });

        // ── ORDERS ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);

            // One User → Many Orders
            e.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            e.Property(o => o.CommissionAmount).HasPrecision(18, 2);
            e.Property(o => o.Discount).HasPrecision(18, 2);
            e.Property(o => o.PaymentMethod).HasMaxLength(10).HasDefaultValue("COD");
            e.Property(o => o.PaymentStatus).HasMaxLength(15).HasDefaultValue("Pending");
            e.Property(o => o.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(o => o.DeliveryAddress).HasMaxLength(400).IsRequired();
            e.Property(o => o.PromoCode).HasMaxLength(50);
        });

        // ── ORDER ITEMS ────────────────────────────────────────────────────
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(oi => oi.Id);

            // One Order → Many OrderItems
            e.HasOne(oi => oi.Order)
             .WithMany(o => o.OrderItems)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            // One Product → Many OrderItems
            e.HasOne(oi => oi.Product)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(oi => oi.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            e.Property(oi => oi.SelectedSize).HasMaxLength(20);
            e.Property(oi => oi.SelectedColor).HasMaxLength(30);
        });

        // ── SERVICE BOOKINGS ───────────────────────────────────────────────
        modelBuilder.Entity<ServiceBooking>(e =>
        {
            e.HasKey(sb => sb.Id);

            e.HasOne(sb => sb.User)
             .WithMany(u => u.ServiceBookings)
             .HasForeignKey(sb => sb.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(sb => sb.ServiceType).HasMaxLength(50).IsRequired();
            e.Property(sb => sb.TimeSlot).HasMaxLength(50);
            e.Property(sb => sb.ServiceAddress).HasMaxLength(400).IsRequired();
            e.Property(sb => sb.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(sb => sb.PaymentMethod).HasMaxLength(10).HasDefaultValue("Cash");
            e.Property(sb => sb.ServiceCharge).HasPrecision(18, 2);
        });

        // ── PROMO CODES ────────────────────────────────────────────────────
        modelBuilder.Entity<PromoCode>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.Code).IsUnique();           // codes must be unique
            e.Property(p => p.Code).HasMaxLength(50).IsRequired();
            e.Property(p => p.DiscountPercent).HasPrecision(5, 2);
        });

        // ── RETURN REQUESTS ────────────────────────────────────────────────
        modelBuilder.Entity<ReturnRequest>(e =>
        {
            e.HasKey(r => r.Id);

            // One Order ↔ One ReturnRequest (one-to-one)
            e.HasOne(r => r.Order)
             .WithOne(o => o.ReturnRequest)
             .HasForeignKey<ReturnRequest>(r => r.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(r => r.Reason).HasMaxLength(500).IsRequired();
            e.Property(r => r.Status).HasMaxLength(15).HasDefaultValue("Pending");
            e.Property(r => r.AdminNote).HasMaxLength(300);
        });

        // ── REVIEWS ────────────────────────────────────────────────────────
        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);

            e.HasOne(r => r.User)
             .WithMany(u => u.Reviews)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            // Review → Product (optional)
            e.HasOne(r => r.Product)
             .WithMany(p => p.Reviews)
             .HasForeignKey(r => r.ProductId)
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired(false);

            // Review → ServiceBooking (optional, one-to-one)
            e.HasOne(r => r.ServiceBooking)
             .WithOne(sb => sb.Review)
             .HasForeignKey<Review>(r => r.ServiceBookingId)
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired(false);

            e.Property(r => r.Comment).HasMaxLength(500);
        });

        // ── SEED DATA (default rows inserted on first migration) ───────────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Default Admin user — phone: 9800000000, password: Admin@123
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            FullName = "sabmero Admin",
            Phone = "9800000000",
            Email = "admin@sabmero.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Address = "Kathmandu, Nepal",
            Role = "Admin",
            IsKycVerified = true,
            IsActive = true,
            CreatedAt = new DateTime(2026, 6, 4, 0, 0, 0, DateTimeKind.Utc)
        });

        // Default categories from the agreement
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", IsActive = true, CreatedAt = new DateTime(2026, 6, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 2, Name = "Clothing", IsActive = true, CreatedAt = new DateTime(2026, 6, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 3, Name = "Footwear", IsActive = true, CreatedAt = new DateTime(2026, 6, 4, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 4, Name = "Grocery", IsActive = true, CreatedAt = new DateTime(2026, 6, 4, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}