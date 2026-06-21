using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Order;

// ── One line the customer wants to buy (sent inside PlaceOrderDto) ──
public class OrderItemInputDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    public string? SelectedSize { get; set; }
    public string? SelectedColor { get; set; }
}

// ── What Flutter SENDS to place an order (the whole cart at once) ──
public class PlaceOrderDto
{
    [Required(ErrorMessage = "Delivery address is required")]
    [MaxLength(400)]
    public string DeliveryAddress { get; set; } = string.Empty;

    // "COD" | "QR"
    public string PaymentMethod { get; set; } = "COD";

    // Optional promo code typed at checkout
    public string? PromoCode { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<OrderItemInputDto> Items { get; set; } = new();
}

// ── One line in the API response ──
public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }       // UnitPrice * Quantity
    public string? SelectedSize { get; set; }
    public string? SelectedColor { get; set; }
}

// ── The full order the API SENDS BACK ──
public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? RiderId { get; set; }
    public string? RiderName { get; set; }
    public decimal SubTotal { get; set; }        // sum of items before discount
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }     // final amount payable
    public decimal CommissionAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentScreenshotPath { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? PromoCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

// ── Admin/rider SENDS this to change an order's status ──
public class UpdateOrderStatusDto
{
    [Required]
    // "Pending" | "Processing" | "Dispatched" | "Delivered" | "Cancelled"
    public string Status { get; set; } = string.Empty;
}

// ── Admin SENDS this to assign a delivery rider ──
public class AssignRiderDto
{
    [Required]
    public int RiderId { get; set; }
}