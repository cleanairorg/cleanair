using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class WeatherStationRepository(MyDbContext ctx) : IWeatherStationRepository
{
    public List<Devicelog> GetRecentLogs()
    {
        return ctx.Devicelogs.ToList();
    }

    public async Task<Devicelog?> GetCurrentLogByDeviceIdAsync(string deviceId)
    {
        return await ctx.Devicelogs
            .Where(d => d.Deviceid == deviceId)
            .OrderByDescending(d => d.Timestamp)
            .FirstOrDefaultAsync();
    }

    public Devicelog AddDeviceLog(Devicelog deviceLog)
    {
        ctx.Devicelogs.Add(deviceLog);
        ctx.SaveChanges();
        return deviceLog;
    }

    public async Task DeleteAllData()
    {
        var allDataLogs = ctx.Devicelogs.ToList();
        ctx.RemoveRange(allDataLogs);
        await ctx.SaveChangesAsync();
    }
}