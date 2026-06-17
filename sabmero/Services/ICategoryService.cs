using sabmero.DTOs.Category;

namespace sabmero.Services;

// Contract for category operations.
public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(bool includeInactive);
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, CategoryDto? Data)> CreateAsync(CreateCategoryDto dto);
    Task<(bool Success, string Message, CategoryDto? Data)> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}