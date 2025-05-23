using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface ICleanAirRepository
{
    List<Devicelog> GetRecentLogs();
    Devicelog GetLatestLogs();
    Task<Devicelog?> GetCurrentLogByDeviceIdAsync(string deviceId);
    Devicelog AddDeviceLog(Devicelog deviceLog);
    Task DeleteAllData();
}