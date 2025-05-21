using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Services;

public class ThresholdService(
    IDeviceThresholdRepository thresholdRepository,
IConnectionManager connectionManager) : IThresholdService
{
    public async Task UpdateThresholdAndBroadcastAsync(AdminUpdatesThresholdsDto adminUpdatesThresholdsDto)
    {
        foreach (var t in adminUpdatesThresholdsDto.Thresholds)
        {
            var threshold = new DeviceThreshold
            {
                Deviceid = adminUpdatesThresholdsDto.DeviceId,
                Metric = t.Metric,
                WarnMin = t.WarnMin,
                WarnMax = t.WarnMax,
                GoodMin = t.GoodMin,
                GoodMax = t.GoodMax,
            };
            
            await thresholdRepository.UpdateThresholdAsync(threshold);
        }
        
        // Then broadcast updated thresholds to Dashboard topic
        var broadcast = new UpdatedThresholdsDto
        {
            DeviceId = adminUpdatesThresholdsDto.DeviceId,
            UpdatedThresholds = adminUpdatesThresholdsDto.Thresholds
        };
        
        await connectionManager.BroadcastToTopic(StringConstants.Dashboard, broadcast);
    }
}