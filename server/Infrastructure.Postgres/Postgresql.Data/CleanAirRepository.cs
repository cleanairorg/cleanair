using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class CleanAirRepository(MyDbContext ctx) : ICleanAirRepository
{
    public List<Devicelog> GetRecentLogs()
    {
        return ctx.Devicelogs.ToList();
    }
    
    public Devicelog GetLatestLogs()
    {
        return ctx.Devicelogs.OrderByDescending(x => x.Timestamp).FirstOrDefault()!;
    }

    public async Task<Devicelog?> GetCurrentLogAsync()
    {
        return await ctx.Devicelogs
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
    
    
    public List<Devicelog> GetLogsForToday(TimeRangeDto dto)
    {
      
        var from = dto.StartDate.ToUniversalTime();
        var to = dto.EndDate.ToUniversalTime();

        return ctx.Devicelogs
            .Where(x => x.Timestamp >= from && x.Timestamp <= to)
            .OrderBy(x => x.Timestamp)
            .ToList();
    }

    public List<Devicelog> GetDailyAverages(TimeRangeDto dto)
    {
        var from = dto.StartDate.ToUniversalTime();
        var to = dto.EndDate.ToUniversalTime();


        return ctx.Devicelogs
            .Where(x =>  x.Timestamp >= from && x.Timestamp <= to)
            .AsEnumerable()
            .GroupBy(x => x.Timestamp.Date)
            .Select(g => new Devicelog
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = g.Key,
                Temperature = g.Average(x => x.Temperature),
                Humidity = g.Average(x => x.Humidity),
                Pressure = g.Average(x => x.Pressure),
                Airquality = (int)g.Average(x => x.Airquality),
                Unit = "Celsius"
            }).ToList();
    }
    
}