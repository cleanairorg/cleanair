using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IDeviceThresholdRepository
{
    Task<List<DeviceThreshold>> GetAllAsync();
    Task UpdateThresholdAsync(DeviceThreshold threshold);
}