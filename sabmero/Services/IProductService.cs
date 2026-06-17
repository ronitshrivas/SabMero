using sabmero.DTOs.Common;
using sabmero.DTOs.Product;

namespace sabmero.Services;

// Contract for product operations.
public interface IProductService
{
    // Public catalogue browse with search / filter / sort / pagination.
    Task<PagedResult<ProductDto>> BrowseAsync(
        string? search, int? categoryId, int? vendorId,
        decimal? minPrice, decimal? maxPrice,
        string? sortBy, int page, int pageSize);

    Task<ProductDto?> GetByIdAsync(int id);

    // Vendor: list only my own products.
    Task<List<ProductDto>> GetByVendorUserAsync(int userId);

    // Vendor: create a product (must be an approved vendor).
    Task<(bool Success, string Message, ProductDto? Data)> CreateAsync(int userId, CreateProductDto dto);

    // Vendor: update one of my products.
    Task<(bool Success, string Message, ProductDto? Data)> UpdateAsync(int userId, string role, int productId, UpdateProductDto dto);

    // Vendor: delete (soft) one of my products.
    Task<(bool Success, string Message)> DeleteAsync(int userId, string role, int productId);
}