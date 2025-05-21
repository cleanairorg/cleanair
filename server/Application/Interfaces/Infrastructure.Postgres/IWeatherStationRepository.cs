using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IWeatherStationRepository
{
    List<Devicelog> GetRecentLogs();
    Devicelog AddDeviceLog(Devicelog deviceLog);
    Task DeleteAllData();
    List<AggregatedLogDto> GetDailyAverages(TimeRangeDto timeRangeDto);
    List<Devicelog> GetLogsForToday(TimeRangeDto timeRangeDto);
}