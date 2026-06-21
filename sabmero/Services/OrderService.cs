using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Order;
using sabmero.Models;

namespace sabmero.Services;

// The heart of the shop. PlaceOrderAsync does a lot:
//  1. Validates every product (exists, active, enough stock)
//  2. Locks in the price at order time
//  3. Applies a promo code if valid
//  4. Calculates platform commission from each vendor's rate
//  5. Deducts stock and saves everything in one transaction
public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext db, ILogger<OrderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, OrderDto? Data)> PlaceOrderAsync(int userId, PlaceOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return (false, "Your cart is empty.", null);

        // Load all referenced products in one query
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Include(p => p.Vendor)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        // Use a DB transaction so stock + order are saved atomically.
        await using var tx = await _db.Database.BeginTransactionAsync();

        decimal subTotal = 0m;
        decimal commissionTotal = 0m;
        var orderItems = new List<OrderItem>();

        foreach (var item in dto.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
                return (false, $"Product #{item.ProductId} no longer exists.", null);
            if (!product.IsActive || !product.Vendor.IsApproved)
                return (false, $"'{product.Name}' is not available.", null);
            if (item.Quantity < 1)
                return (false, $"Invalid quantity for '{product.Name}'.", null);
            if (product.Stock < item.Quantity)
                return (false, $"Not enough stock for '{product.Name}'. Only {product.Stock} left.", null);

            var lineTotal = product.Price * item.Quantity;
            subTotal += lineTotal;

            // Commission the platform earns from this vendor for this line.
            commissionTotal += lineTotal * (product.Vendor.CommissionRate / 100m);

            // Deduct stock
            product.Stock -= item.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                SelectedSize = item.SelectedSize,
                SelectedColor = item.SelectedColor
            });
        }

        // ── Promo code ──
        decimal discount = 0m;
        string? appliedPromo = null;
        if (!string.IsNullOrWhiteSpace(dto.PromoCode))
        {
            var code = dto.PromoCode.Trim().ToUpper();
            var promo = await _db.PromoCodes
                .FirstOrDefaultAsync(p => p.Code.ToUpper() == code && p.IsActive);

            if (promo == null)
                return (false, "Invalid promo code.", null);
            if (promo.ExpiresAt < DateTime.UtcNow)
                return (false, "This promo code has expired.", null);

            discount = Math.Round(subTotal * (promo.DiscountPercent / 100m), 2);
            appliedPromo = promo.Code;
        }

        var total = subTotal - discount;
        if (total < 0) total = 0;

        var order = new Order
        {
            UserId = userId,
            TotalAmount = total,
            CommissionAmount = Math.Round(commissionTotal, 2),
            PaymentMethod = dto.PaymentMethod == "QR" ? "QR" : "COD",
            PaymentStatus = "Pending",
            Status = "Pending",
            DeliveryAddress = dto.DeliveryAddress.Trim(),
            PromoCode = appliedPromo,
            Discount = discount,
            CreatedAt = DateTime.UtcNow,
            OrderItems = orderItems
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _logger.LogInformation("Order {Id} placed by user {UserId}, total {Total}", order.Id, userId, total);

        var dtoResult = await BuildOrderDtoAsync(order.Id);
        return (true, "Order placed successfully.", dtoResult);
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync(int userId)
    {
        var ids = await _db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => o.Id)
            .ToListAsync();

        return await BuildManyAsync(ids);
    }

    public async Task<(bool Success, string Message, OrderDto? Data)> GetByIdAsync(int userId, string role, int orderId)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null)
            return (false, "Order not found.", null);

        // Customers can only view their own orders. Admin/Rider can view any.
        if (role == "Customer" && order.UserId != userId)
            return (false, "You can only view your own orders.", null);

        return (true, "Found.", await BuildOrderDtoAsync(orderId));
    }

    public async Task<List<OrderDto>> GetAllAsync(string? status)
    {
        var q = _db.Orders.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(o => o.Status == status);

        var ids = await q.OrderByDescending(o => o.CreatedAt).Select(o => o.Id).ToListAsync();
        return await BuildManyAsync(ids);
    }

    public async Task<List<OrderDto>> GetVendorOrdersAsync(int userId)
    {
        var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.UserId == userId);
        if (vendor == null) return new List<OrderDto>();

        // Orders that contain at least one product from this vendor.
        var ids = await _db.Orders
            .Where(o => o.OrderItems.Any(oi => oi.Product.VendorId == vendor.Id))
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => o.Id)
            .ToListAsync();

        return await BuildManyAsync(ids);
    }

    public async Task<(bool Success, string Message)> UpdateStatusAsync(int orderId, string status)
    {
        var allowed = new[] { "Pending", "Processing", "Dispatched", "Delivered", "Cancelled" };
        if (!allowed.Contains(status))
            return (false, "Invalid status.");

        var order = await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            return (false, "Order not found.");

        // A QR order can't move forward until the admin/vendor has verified the
        // payment screenshot. Cancelling is still allowed.
        if (order.PaymentMethod == "QR"
            && order.PaymentStatus != "Verified" && order.PaymentStatus != "Paid"
            && status != "Cancelled" && status != "Pending")
        {
            return (false, "Payment is not verified yet. This order can't be processed until the payment screenshot is approved.");
        }

        // If cancelling, return the stock back to products.
        if (status == "Cancelled" && order.Status != "Cancelled")
        {
            foreach (var item in order.OrderItems)
            {
                var product = await _db.Products.FindAsync(item.ProductId);
                if (product != null) product.Stock += item.Quantity;
            }
        }

        order.Status = status;
        if (status == "Delivered")
            order.PaymentStatus = "Paid";   // COD collected on delivery

        await _db.SaveChangesAsync();
        return (true, $"Order status updated to {status}.");
    }

    public async Task<(bool Success, string Message)> AssignRiderAsync(int orderId, int riderId)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null)
            return (false, "Order not found.");

        var rider = await _db.Users.FindAsync(riderId);
        if (rider == null || rider.Role != "Rider")
            return (false, "That user is not a rider.");

        // Don't dispatch a QR order whose payment hasn't been verified yet.
        if (order.PaymentMethod == "QR"
            && order.PaymentStatus != "Verified" && order.PaymentStatus != "Paid")
        {
            return (false, "Payment is not verified yet. Verify the payment before assigning a rider.");
        }

        order.RiderId = riderId;
        if (order.Status == "Pending" || order.Status == "Processing")
            order.Status = "Dispatched";

        await _db.SaveChangesAsync();
        return (true, "Rider assigned.");
    }

    public async Task<List<OrderDto>> GetRiderOrdersAsync(int riderId)
    {
        var ids = await _db.Orders
            .Where(o => o.RiderId == riderId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => o.Id)
            .ToListAsync();

        return await BuildManyAsync(ids);
    }

    public async Task<(bool Success, string Message)> CancelAsync(int userId, int orderId)
    {
        var order = await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            return (false, "Order not found.");
        if (order.UserId != userId)
            return (false, "You can only cancel your own orders.");
        if (order.Status != "Pending" && order.Status != "Processing")
            return (false, "This order can no longer be cancelled.");

        foreach (var item in order.OrderItems)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product != null) product.Stock += item.Quantity;
        }

        order.Status = "Cancelled";
        await _db.SaveChangesAsync();
        return (true, "Order cancelled.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private async Task<List<OrderDto>> BuildManyAsync(List<int> ids)
    {
        var result = new List<OrderDto>();
        foreach (var id in ids)
        {
            var dto = await BuildOrderDtoAsync(id);
            if (dto != null) result.Add(dto);
        }
        return result;
    }

    private async Task<OrderDto?> BuildOrderDtoAsync(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return null;

        string? riderName = null;
        if (order.RiderId.HasValue)
            riderName = (await _db.Users.FindAsync(order.RiderId.Value))?.FullName;

        var items = order.OrderItems.Select(oi => new OrderItemDto
        {
            Id = oi.Id,
            ProductId = oi.ProductId,
            ProductName = oi.Product.Name,
            ProductImage = oi.Product.ImagePath,
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice,
            LineTotal = oi.UnitPrice * oi.Quantity,
            SelectedSize = oi.SelectedSize,
            SelectedColor = oi.SelectedColor
        }).ToList();

        var subTotal = items.Sum(i => i.LineTotal);

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            CustomerName = order.User.FullName,
            RiderId = order.RiderId,
            RiderName = riderName,
            SubTotal = subTotal,
            Discount = order.Discount,
            TotalAmount = order.TotalAmount,
            CommissionAmount = order.CommissionAmount,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            PaymentScreenshotPath = order.PaymentScreenshotPath,
            Status = order.Status,
            DeliveryAddress = order.DeliveryAddress,
            PromoCode = order.PromoCode,
            CreatedAt = order.CreatedAt,
            Items = items
        };
    }
}