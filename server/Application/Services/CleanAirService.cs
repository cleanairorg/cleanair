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
        try
        {
            logger.LogInformation("[CleanAirService] Attempting to get the latest device log");
            var latestLog = cleanAirRepository.GetLatestLogs();
            
            if (latestLog == null)
            {
                logger.LogWarning("[CleanAirService] No device logs found");
            }
            else
            {
                logger.LogInformation($"[CleanAirService] got the latest device log found, LogID: {latestLog.Id}");
            }
            
            return latestLog;
        }
        catch (Exception ex)
        {
            logger.LogError("[CleanAirService] Failed to get the latest log", ex);
            throw;
        }
    }


    public List<Devicelog> GetDailyAverages(TimeRangeDto timeRangeDto)
    {
        ArgumentNullException.ThrowIfNull(timeRangeDto);
        
        if (timeRangeDto.StartDate >= timeRangeDto.EndDate)
            throw new ArgumentException("StartDate cannot be after EndDate.");
    
        logger.LogInformation($"[CleanAir Service] GetDailyAverages with DTO: {JsonSerializer.Serialize(timeRangeDto)}");
        var result = cleanAirRepository.GetDailyAverages(timeRangeDto);
        logger.LogInformation($"[CleanAir Service] GetDailyAverages returned {result.Count} records.");
        return result;
    }

    
    public List<Devicelog> GetLogsForToday(TimeRangeDto timeRangeDto)
    {
        try
        {
            if (timeRangeDto.StartDate > timeRangeDto.EndDate)
                throw new ArgumentException("StartDate cannot be after EndDate.");

            logger.LogInformation($"[CleanAir Service] GetLogsForToday called with DTO: {JsonSerializer.Serialize(timeRangeDto)}");

            var logs = cleanAirRepository.GetLogsForToday(timeRangeDto);

            logger.LogInformation($"[CleanAir Service] GetLogsForToday returned {logs.Count} records.");
            return logs;
        }
        catch (Exception ex)
        {
            logger.LogError($"[CleanAir Service] Error in GetLogsForToday with DTO: {JsonSerializer.Serialize(timeRangeDto)}", ex);
            throw;
        }
    }

    public Task UpdateDeviceIntervalAndBroadcast(AdminChangesDeviceIntervalDto dto)
    {
        try
        {
            logger.LogInformation("[CleanAirService] Updating device interval");
            mqttPublisher.Publish(dto.Interval, StringConstants.ChangeInterval);

            var broadcast = new ServerBroadcastsIntervalChange()
            {
                Interval = dto.Interval
            };
            logger.LogInformation($"[CleanAirService] broadcasting interval changes to dashboard");
            connectionManager.BroadcastToTopic(StringConstants.Dashboard, broadcast);
            logger.LogInformation("[CleanAirService] Updated interval");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError("[CleanAirService] An error occurred while updating device interval", ex);
            throw;
        }
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
        try
        {
            logger.LogInformation("[CleanAirService] Publishing 'GetMeasurementNowAndBroadcast' message to device");
            await mqttPublisher.Publish("1", StringConstants.GetMeasurementsNow);
            
            logger.LogInformation("[CleanAirService] Getting the latest device logs");
            var recentLogs = cleanAirRepository.GetLatestLogs();
            
            var broadcast = new ServerBroadcastsLatestReqestedMeasurement()
            {
                LatestMeasurement = recentLogs
            };
            
            logger.LogInformation("[CleanAirService] Broadcasting the latest measurement to dashboard");
            await connectionManager.BroadcastToTopic(StringConstants.Dashboard, broadcast);
        }
        catch (Exception ex)
        {
            logger.LogError("[CleanAirService] An error occured while executing GetMeasurementNowAndBroadcast", ex);
        }
    }
}

public class AdminHasDeletedData : ApplicationBaseDto
{
    public override string eventType { get; set; } = nameof(AdminHasDeletedData);
}