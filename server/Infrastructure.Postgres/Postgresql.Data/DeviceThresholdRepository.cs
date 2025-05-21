using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class DeviceThresholdRepository (MyDbContext context) : IDeviceThresholdRepository
{
    public async Task<List<DeviceThreshold>> GetByDeviceIdAsync(string deviceId)
    {
        return await context.DeviceThresholds
            .Where(t => t.Deviceid == deviceId)
            .ToListAsync();
    }

    public async Task UpdateThresholdAsync(DeviceThreshold threshold)
    {
        var existing = await context.DeviceThresholds
            .FirstOrDefaultAsync(t => 
                t.Deviceid == threshold.Deviceid && 
                t.Metric == threshold.Metric);

        if (existing is null)
        {
            threshold.Id = Guid.NewGuid().ToString();
            await context.DeviceThresholds.AddAsync(threshold);
        }
        else
        {
            existing.WarnMin = threshold.WarnMin;
            existing.WarnMax = threshold.WarnMax;
            existing.GoodMin = threshold.GoodMin;
            existing.GoodMax = threshold.GoodMax;
        }
        await context.SaveChangesAsync();
    }
}