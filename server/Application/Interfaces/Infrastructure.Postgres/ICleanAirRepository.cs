using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface ICleanAirRepository
{
    List<Devicelog> GetRecentLogs();
    Devicelog GetLatestLogs();
    Devicelog AddDeviceLog(Devicelog deviceLog);
    Task DeleteAllData();
    List<Devicelog> GetDailyAverages(TimeRangeDto dto);
    List<Devicelog> GetLogsForToday(TimeRangeDto timeRangeDto);
}