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

public class WeatherStationService(
    IOptionsMonitor<AppOptions> optionsMonitor,
    ILogger<WeatherStationService> logger,
    IWeatherStationRepository weatherStationRepository,
    IMqttPublisher mqttPublisher,
    IConnectionManager connectionManager) : IWeatherStationService
{
    public Task AddToDbAndBroadcast(DeviceLogDto? dto)
    {
        var deviceLog = new Devicelog
        {
            Timestamp = DateTime.UtcNow,
            Temperature = dto.Value,
            Humidity = dto.Value,
            Pressure = dto.Value,
            Airquality = dto.Value,
            Deviceid = dto.DeviceId,
            Unit = dto.Unit,
            Id = Guid.NewGuid()
                .ToString()
        };
        weatherStationRepository.AddDeviceLog(deviceLog);
        var recentLogs = weatherStationRepository.GetRecentLogs();
        var broadcast = new ServerBroadcastsLiveDataToDashboard
        {
            Logs = recentLogs
        };
        connectionManager.BroadcastToTopic(StringConstants.Dashboard, broadcast);
        return Task.CompletedTask;
    }

    public List<Devicelog> GetDeviceFeed(JwtClaims client)
    {
        return weatherStationRepository.GetRecentLogs();
    }
    

    public List<AggregatedLogDto> GetDailyAverages(TimeRangeDto dto)
    {
        var from = dto.StartDate.ToUniversalTime();
        var to = dto.EndDate.ToUniversalTime();

        if (dto.StartDate > dto.EndDate)
        {
            logger.LogError("Start date cannot be greater than end date");
            throw new ArgumentException("StartDate cannot be after EndDate.");
        }
            

        
        return weatherStationRepository.GetDailyAverages(new TimeRangeDto
        {
            StartDate = from,
            EndDate = to
        });
    }
    
    public Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims)
    {
        mqttPublisher.Publish(dto, StringConstants.Device + $"/{dto.DeviceId}/" + StringConstants.ChangePreferences);
        return Task.CompletedTask;
    }

    public async Task DeleteDataAndBroadcast(JwtClaims jwt)
    {
        await weatherStationRepository.DeleteAllData();
        await connectionManager.BroadcastToTopic(StringConstants.Dashboard, new AdminHasDeletedData());
    }

    public async Task GetMeasurementNowAndBroadcast()
    {
        await mqttPublisher.Publish("1", "cleanair/measurement/now");
    }
}

public class AdminHasDeletedData : ApplicationBaseDto
{
    public override string eventType { get; set; } = nameof(AdminHasDeletedData);
}