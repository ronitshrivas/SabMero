using sabmero.DTOs.Common;

namespace sabmero.Services;

// App-wide settings the admin controls (currently the delivery charge).
public interface ISettingsService
{
    Task<DeliverySettingsDto> GetDeliveryAsync();
    Task<(bool Success, string Message)> SetDeliveryAsync(decimal deliveryFee, decimal freeDeliveryAbove);
}
