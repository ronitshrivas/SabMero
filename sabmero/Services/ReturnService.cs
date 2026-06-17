using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Return;
using sabmero.Models;

namespace sabmero.Services;

// Customers raise return requests on delivered orders; admin approves/rejects.
public class ReturnService : IReturnService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ReturnService> _logger;

    public ReturnService(AppDbContext db, ILogger<ReturnService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, ReturnDto? Data)> CreateAsync(int userId, CreateReturnDto dto)
    {
        var order = await _db.Orders
            .Include(o => o.ReturnRequest)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order == null)
            return (false, "Order not found.", null);
        if (order.UserId != userId)
            return (false, "You can only request returns on your own orders.", null);
        if (order.Status != "Delivered")
            return (false, "Returns can only be requested on delivered orders.", null);
        if (order.ReturnRequest != null)
            return (false, "A return request already exists for this order.", null);

        var rr = new ReturnRequest
        {
            OrderId = dto.OrderId,
            Reason = dto.Reason.Trim(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.ReturnRequests.Add(rr);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Return request {Id} created for order {OrderId}", rr.Id, dto.OrderId);

        return (true, "Return request submitted.", await BuildAsync(rr.Id));
    }

    public async Task<List<ReturnDto>> GetMyReturnsAsync(int userId)
    {
        var ids = await _db.ReturnRequests
            .Where(r => r.Order.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => r.Id).ToListAsync();
        return await BuildManyAsync(ids);
    }

    public async Task<List<ReturnDto>> GetAllAsync(string? status)
    {
        var q = _db.ReturnRequests.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(r => r.Status == status);

        var ids = await q.OrderByDescending(r => r.CreatedAt).Select(r => r.Id).ToListAsync();
        return await BuildManyAsync(ids);
    }

    public async Task<(bool Success, string Message)> ResolveAsync(int returnId, ResolveReturnDto dto)
    {
        if (dto.Status != "Approved" && dto.Status != "Rejected")
            return (false, "Status must be Approved or Rejected.");

        var rr = await _db.ReturnRequests
            .Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.Id == returnId);
        if (rr == null)
            return (false, "Return request not found.");

        rr.Status = dto.Status;
        rr.AdminNote = dto.AdminNote;

        // On approval, mark the order's payment as refunded.
        if (dto.Status == "Approved")
            rr.Order.PaymentStatus = "Refunded";

        await _db.SaveChangesAsync();
        return (true, $"Return {dto.Status.ToLower()}.");
    }

    private async Task<List<ReturnDto>> BuildManyAsync(List<int> ids)
    {
        var result = new List<ReturnDto>();
        foreach (var id in ids)
        {
            var dto = await BuildAsync(id);
            if (dto != null) result.Add(dto);
        }
        return result;
    }

    private async Task<ReturnDto?> BuildAsync(int id)
        => await _db.ReturnRequests
            .Where(r => r.Id == id)
            .Select(r => new ReturnDto
            {
                Id = r.Id,
                OrderId = r.OrderId,
                CustomerName = r.Order.User.FullName,
                OrderTotal = r.Order.TotalAmount,
                Reason = r.Reason,
                Status = r.Status,
                AdminNote = r.AdminNote,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();
}