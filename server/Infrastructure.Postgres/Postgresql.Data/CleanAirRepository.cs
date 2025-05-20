using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;

namespace Infrastructure.Postgres.Postgresql.Data;

public class CleanAirRepository(MyDbContext ctx) : ICleanAirRepository
{
    public List<Devicelog> GetRecentLogs()
    {
        return ctx.Devicelogs.ToList();
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