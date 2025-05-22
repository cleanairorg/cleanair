using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface ICleanAirRepository
{
    List<Devicelog> GetRecentLogs();
    Devicelog GetLatestLogs();
    Devicelog AddDeviceLog(Devicelog deviceLog);
    Task DeleteAllData();
}