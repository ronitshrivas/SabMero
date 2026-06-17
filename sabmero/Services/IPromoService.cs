using sabmero.DTOs.Promo;

namespace sabmero.Services;

// Contract for promo code operations.
public interface IPromoService
{
    Task<List<PromoDto>> GetAllAsync();
    Task<(bool Success, string Message, PromoDto? Data)> CreateAsync(CreatePromoDto dto);
    Task<(bool Success, string Message, PromoDto? Data)> UpdateAsync(int id, UpdatePromoDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);

    // Customer-facing: check a code against a cart subtotal.
    Task<(bool Success, string Message, PromoValidationResult? Data)> ValidateAsync(ValidatePromoDto dto);
}