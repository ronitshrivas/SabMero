using sabmero.DTOs.Return;

namespace sabmero.Services;

// Contract for return/refund request operations.
public interface IReturnService
{
    // Customer requests a return on one of their delivered orders.
    Task<(bool Success, string Message, ReturnDto? Data)> CreateAsync(int userId, CreateReturnDto dto);

    // Customer: my return requests.
    Task<List<ReturnDto>> GetMyReturnsAsync(int userId);

    // Admin: all return requests (optionally filter by status).
    Task<List<ReturnDto>> GetAllAsync(string? status);

    // Admin: approve or reject a return.
    Task<(bool Success, string Message)> ResolveAsync(int returnId, ResolveReturnDto dto);
}