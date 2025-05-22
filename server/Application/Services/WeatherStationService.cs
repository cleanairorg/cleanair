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
        try
        {
            if (dto.StartDate > dto.EndDate)
                throw new ArgumentException("StartDate cannot be after EndDate.");

            logger.LogInformation($"[Service] GetDailyAverages called with DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");

            var result = weatherStationRepository.GetDailyAverages(dto);

            logger.LogInformation($"[Service] GetDailyAverages returned {result.Count} records.");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError($"[Service] Error in GetDailyAverages with DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}", ex);
            throw;
        }
    }


    public List<Devicelog> GetLogsForToday(TimeRangeDto dto)
    {
        try
        {
            if (dto.StartDate > dto.EndDate)
                throw new ArgumentException("StartDate cannot be after EndDate.");

            logger.LogInformation($"[Service] GetLogsForToday called with DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");

            var logs = weatherStationRepository.GetLogsForToday(dto);

            logger.LogInformation($"[Service] GetLogsForToday returned {logs.Count} records.");
            return logs;
        }
        catch (Exception ex)
        {
            logger.LogError($"[Service] Error in GetLogsForToday with DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}", ex);
            throw;
        }
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