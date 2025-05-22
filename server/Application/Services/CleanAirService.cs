using Application.Interfaces;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class CleanAirService(
    IOptionsMonitor<AppOptions> optionsMonitor,
    ILogger<CleanAirService> logger,
    ICleanAirRepository cleanAirRepository,
    IMqttPublisher mqttPublisher,
    IConnectionManager connectionManager) : ICleanAirService
{
    public Task AddToDbAndBroadcast(CollectDataDto? dto)
    {
        
        if (dto == null)
        {
            logger.LogWarning("CleanAirService: AddToDbAndBroadcast, dto is null");
            return Task.CompletedTask;
        }
        
        var deviceLog = new Devicelog
        {
            Id = Guid.NewGuid().ToString(),
            Deviceid = dto.DeviceId,
            Timestamp = DateTime.UtcNow,
            Temperature = (decimal)dto.Temperature,
            Humidity = (decimal)dto.Humidity,
            Pressure = (decimal)dto.Pressure,
            Airquality = (int)dto.AirQuality,
            Unit = "Celsius"
        };
        logger.LogInformation("CleanAirService: AddToDbAndBroadcast, Added DeviceLog to Database");
        cleanAirRepository.AddDeviceLog(deviceLog);
        var recentLogs = cleanAirRepository.GetRecentLogs();
        var broadcast = new ServerBroadcastsLiveDataToDashboard
        {
            Logs = recentLogs
        };
        connectionManager.BroadcastToTopic(StringConstants.Dashboard, broadcast);
        return Task.CompletedTask;
    }

    public List<Devicelog> GetDeviceFeed(JwtClaims client)
    {
        return cleanAirRepository.GetRecentLogs();
    }


    public Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims)
    {
        mqttPublisher.Publish(dto, StringConstants.Device + $"/{dto.DeviceId}/" + StringConstants.ChangePreferences);
        return Task.CompletedTask;
    }

    public async Task DeleteDataAndBroadcast(JwtClaims jwt)
    {
        await cleanAirRepository.DeleteAllData();
        await connectionManager.BroadcastToTopic(StringConstants.Dashboard, new AdminHasDeletedData());
    }
}

public class AdminHasDeletedData : ApplicationBaseDto
{
    public override string eventType { get; set; } = nameof(AdminHasDeletedData);
}