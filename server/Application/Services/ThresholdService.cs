using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.SharedDtos;
using Core.Domain.Entities;

namespace Application.Services;

public class ThresholdService(
    IDeviceThresholdRepository thresholdRepository,
    ICleanAirRepository cleanairRepository,
    IConnectionManager connectionManager,
    IThresholdEvaluator evaluator) : IThresholdService
{
    public async Task UpdateThresholdsAndBroadcastAsync(AdminUpdatesThresholdsDto adminUpdatesThresholdsDto)
    {
        var currentLog =
            await cleanairRepository.GetCurrentLogByDeviceIdAsync(adminUpdatesThresholdsDto.DeviceId);
        if (currentLog == null) throw new Exception("No current log found for device");

        var updatedThresholds = new List<DeviceThreshold>();


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
            updatedThresholds.Add(threshold);
        }

        var evalations = EvaluateThresholdsAgainstLog(currentLog, updatedThresholds);

        await connectionManager.BroadcastToTopic(StringConstants.Dashboard, new ThresholdsBroadcastDto
        {
            DeviceId = adminUpdatesThresholdsDto.DeviceId,
            UpdatedThresholds = adminUpdatesThresholdsDto.Thresholds,
            Evaluations = evalations
        });
    }

    public async Task<ThresholdsBroadcastDto> GetThresholdsWithEvaluationAsync(string deviceId)
    {
        var thresholds = await thresholdRepository.GetByDeviceIdAsync(deviceId);
        var log = await cleanairRepository.GetCurrentLogByDeviceIdAsync(deviceId);
        if (log is null) throw new Exception("No current log found for device");
        
        var evaluations = EvaluateThresholdsAgainstLog(log, thresholds);

        return new ThresholdsBroadcastDto
        {
            DeviceId = deviceId,
            UpdatedThresholds = thresholds.Select(t => new ThresholdDto
            {
                Metric = t.Metric,
                WarnMin = t.WarnMin,
                WarnMax = t.WarnMax,
                GoodMin = t.GoodMin,
                GoodMax = t.GoodMax,
            }).ToList(),
            Evaluations = evaluations
        };


    }

    private List<ThresholdEvaluationResult> EvaluateThresholdsAgainstLog(
        Devicelog log,
        IEnumerable<DeviceThreshold> thresholds)
    {
        var result = new List<ThresholdEvaluationResult>();

        foreach (var threshold in thresholds)
        {
            var value = threshold.Metric switch
            {
                "temperature" => log.Temperature,
                "humidity" => log.Humidity,
                "pressure" => log.Pressure,
                "airguality" => log.Airquality,
                _ => throw new Exception("Unknown metric " + threshold.Metric)
            };
            
            result.Add(evaluator.Evaluate(threshold.Metric, value, threshold));
        }
        return result;
    }
    
}