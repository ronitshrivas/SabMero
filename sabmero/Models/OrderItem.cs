namespace sabmero.Models;

// One product line inside an Order.
// UnitPrice is saved at order time so price changes don't affect history.
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }        // FK → Orders
    public int ProductId { get; set; }      // FK → Products
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }  // price at time of ordering
    public string? SelectedSize { get; set; }
    public string? SelectedColor { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}