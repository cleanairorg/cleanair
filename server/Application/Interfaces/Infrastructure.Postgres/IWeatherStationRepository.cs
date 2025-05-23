using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IWeatherStationRepository
{
    List<Devicelog> GetRecentLogs();
    Task<Devicelog?> GetCurrentLogByDeviceIdAsync(string deviceId);
    Devicelog AddDeviceLog(Devicelog deviceLog);
    Task DeleteAllData();
}