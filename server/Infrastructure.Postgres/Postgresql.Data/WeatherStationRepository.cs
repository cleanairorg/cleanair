using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos;
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
    
    
    public List<AggregatedLogDto> GetDailyAverages(TimeRangeDto dto)
    {
        var from = dto.StartDate.ToUniversalTime();
        var to = dto.EndDate.ToUniversalTime();
    
        return ctx.Devicelogs
            .Where(x => x.Timestamp >= from && x.Timestamp <= to)
            .AsEnumerable()
            .GroupBy(x => x.Timestamp.Date)
            .Select(g => new AggregatedLogDto {
                Date = g.Key,
                AvgTemperature = g.Average(x => x.Temperature),
                AvgHumidity = g.Average(x => x.Humidity),
                AvgPressure = g.Average(x => x.Pressure),
                AvgAirQuality = g.Average(x => x.Airquality)
            }).ToList();
    }




    
}