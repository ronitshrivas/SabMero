using System.Globalization;
using Microsoft.EntityFrameworkCore;
using sabmero.Data;
using sabmero.DTOs.Common;
using sabmero.Models;

namespace sabmero.Services;

// Reads/writes admin-controlled settings in the AppSettings key-value table.
public class SettingsService : ISettingsService
{
    private const string FeeKey = "DeliveryFee";
    private const string FreeKey = "FreeDeliveryAbove";

    private readonly AppDbContext _db;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(AppDbContext db, ILogger<SettingsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DeliverySettingsDto> GetDeliveryAsync()
    {
        var fee = Parse((await _db.AppSettings.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Key == FeeKey))?.Value);
        var freeAbove = Parse((await _db.AppSettings.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Key == FreeKey))?.Value);
        return new DeliverySettingsDto { DeliveryFee = fee, FreeDeliveryAbove = freeAbove };
    }

    public async Task<(bool Success, string Message)> SetDeliveryAsync(decimal deliveryFee, decimal freeDeliveryAbove)
    {
        if (deliveryFee < 0 || freeDeliveryAbove < 0)
            return (false, "Values cannot be negative.");

        await UpsertAsync(FeeKey, deliveryFee.ToString(CultureInfo.InvariantCulture));
        await UpsertAsync(FreeKey, freeDeliveryAbove.ToString(CultureInfo.InvariantCulture));
        await _db.SaveChangesAsync();
        _logger.LogInformation("Delivery settings updated: fee={Fee}, freeAbove={Free}", deliveryFee, freeDeliveryAbove);
        return (true, "Delivery settings updated.");
    }

    private async Task UpsertAsync(string key, string value)
    {
        var row = await _db.AppSettings.FirstOrDefaultAsync(a => a.Key == key);
        if (row == null)
            _db.AppSettings.Add(new AppSetting { Key = key, Value = value, UpdatedAt = DateTime.UtcNow });
        else { row.Value = value; row.UpdatedAt = DateTime.UtcNow; }
    }

    private static decimal Parse(string? v)
        => decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;
}
