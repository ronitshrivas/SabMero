using sabmero.DTOs.Service;

namespace sabmero.Services;

// Admin-managed catalog of repair/installation services shown in the app.
public interface IRepairServiceCatalog
{
    Task<List<RepairServiceDto>> GetAllAsync(bool includeInactive);
    Task<(bool Success, string Message, RepairServiceDto? Data)> CreateAsync(CreateRepairServiceDto dto);
    Task<(bool Success, string Message)> UpdateAsync(int id, UpdateRepairServiceDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
