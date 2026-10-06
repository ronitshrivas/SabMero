using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Common;

// Delivery charge settings the admin controls. Stored in AppSettings under
// keys "DeliveryFee" and "FreeDeliveryAbove".
public class DeliverySettingsDto
{
    public decimal DeliveryFee { get; set; }          // flat delivery charge
    public decimal FreeDeliveryAbove { get; set; }    // subtotal at/above which delivery is free (0 = disabled)
}

// Admin SENDS this to update the delivery settings.
public class SetDeliverySettingsDto
{
    [Range(0, 1000000, ErrorMessage = "Delivery fee cannot be negative.")]
    public decimal DeliveryFee { get; set; }

    [Range(0, 100000000, ErrorMessage = "Threshold cannot be negative.")]
    public decimal FreeDeliveryAbove { get; set; }
}
