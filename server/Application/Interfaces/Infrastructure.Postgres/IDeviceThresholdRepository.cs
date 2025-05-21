using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IDeviceThresholdRepository
{
    Task<List<DeviceThreshold>> GetByDeviceIdAsync(string deviceId);
    Task UpdateThresholdAsync(DeviceThreshold threshold);
}