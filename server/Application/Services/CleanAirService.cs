using System.Text.Json;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

using Microsoft.Extensions.Options;

namespace Application.Services;

public class CleanAirService(
    IOptionsMonitor<AppOptions> optionsMonitor,
    ILoggingService logger,
    ICleanAirRepository cleanAirRepository,
    IMqttPublisher mqttPublisher,
    IConnectionManager connectionManager) : ICleanAirService
{
    public Task AddToDbAndBroadcast(CollectDataDto? dto)
    {
        if (dto == null)
        {
            logger.LogWarning("[CleanAirService] AddToDbAndBroadcast, dto is null");
            return Task.CompletedTask;
        }

        try
        {
            var deviceLog = new Devicelog
            {
                Id = Guid.NewGuid().ToString(),
                Deviceid = dto.DeviceId,
                Timestamp = DateTime.UtcNow,
                Temperature = (decimal)dto.Temperature,
                Humidity = (decimal)dto.Humidity,
                Pressure = (decimal)dto.Pressure,
                Airquality = (int)dto.AirQuality,
                Unit = "Celsius",
                Interval = dto.Interval
            };

            cleanAirRepository.AddDeviceLog(deviceLog);
            logger.LogInformation("[CleanAirService] AddToDbAndBroadcast, Added DeviceLog to Database");

            var recentLogs = cleanAirRepository.GetLatestLogs();
            var broadcast = new ServerBroadcastsLatestReqestedMeasurement
            {
                LatestMeasurement = recentLogs
            };

            connectionManager.BroadcastToTopic(StringConstants.Dashboard, broadcast);
            logger.LogInformation("[CleanAirService] AddToDbAndBroadcast, Broadcasted latest measurement to dashboard");
        }
        catch (Exception ex)
        {
            logger.LogError("[CleanAirService] AddToDbAndBroadcast, an error occurred while adding to DB and broadcasting", ex);
        }

        return Task.CompletedTask;
    }

    public List<Devicelog> GetDeviceFeed(JwtClaims client)
    {
        return cleanAirRepository.GetRecentLogs();
    }

    public Devicelog GetLatestDeviceLog()
    {
        var latestLog = cleanAirRepository.GetLatestLogs();

        return latestLog;
    }


    public List<Devicelog> GetDailyAverages(TimeRangeDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (dto.StartDate >= dto.EndDate)
            throw new ArgumentException("StartDate cannot be after EndDate.");
    
        logger.LogInformation($"[CleanAir Service] GetDailyAverages with DTO: {JsonSerializer.Serialize(dto)}");
        var result = cleanAirRepository.GetDailyAverages(dto);
        logger.LogInformation($"[CleanAir Service] GetDailyAverages returned {result.Count} records.");
        return result;
    }

    
    public List<Devicelog> GetLogsForToday(TimeRangeDto dto)
    {
        try
        {
            if (dto.StartDate > dto.EndDate)
                throw new ArgumentException("StartDate cannot be after EndDate.");

            logger.LogInformation($"[CleanAir Service] GetLogsForToday called with DTO: {JsonSerializer.Serialize(dto)}");

            var logs = cleanAirRepository.GetLogsForToday(dto);

            logger.LogInformation($"[CleanAir Service] GetLogsForToday returned {logs.Count} records.");
            return logs;
        }
        catch (Exception ex)
        {
            logger.LogError($"[CleanAir Service] Error in GetLogsForToday with DTO: {JsonSerializer.Serialize(dto)}", ex);
            throw;
        }
    }

    public Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims)
    {
        mqttPublisher.Publish(dto, StringConstants.Device + $"/{dto.DeviceId}/" + StringConstants.ChangePreferences);
        return Task.CompletedTask;
    }

    public Task UpdateDeviceIntervalAndBroadcast(AdminChangesDeviceIntervalDto dto)
    {
        mqttPublisher.Publish(dto.Interval, StringConstants.ChangeInterval);

        var broadcast = new ServerBroadcastsIntervalChange()
        {
            Interval = dto.Interval
        };
        connectionManager.BroadcastToTopic(StringConstants.Dashboard, broadcast);
        return Task.CompletedTask;
    }

    public async Task DeleteDataAndBroadcast(JwtClaims jwt)
    {
        try
        {
            await cleanAirRepository.DeleteAllData();
            logger.LogInformation("[CleanAirService] All data for device deleted");
            await connectionManager.BroadcastToTopic(StringConstants.Dashboard, new AdminHasDeletedData());
        }
        catch (Exception ex)
        {
            logger.LogError("[CleanAirService] Failed to delete data.", ex);
            throw;
        }
    }

    public async Task GetMeasurementNowAndBroadcast()
    {
        await mqttPublisher.Publish("1", StringConstants.GetMeasurementsNow);

        var recentLogs = cleanAirRepository.GetLatestLogs();
        var broadcast = new ServerBroadcastsLatestReqestedMeasurement()
        {
            LatestMeasurement = recentLogs
        };
        await connectionManager.BroadcastToTopic(StringConstants.Dashboard, broadcast);
    }
}

public class AdminHasDeletedData : ApplicationBaseDto
{
    public override string eventType { get; set; } = nameof(AdminHasDeletedData);
}