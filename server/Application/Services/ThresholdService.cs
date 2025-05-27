using Application.Interfaces;
using Application.Interfaces.Infrastructure.MQTT;
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
    ICleanAirRepository cleanAirRepository,
    IConnectionManager connectionManager,
    IMqttPublisher mqttPublisher,
    IThresholdEvaluator evaluator) : IThresholdService
{
    public async Task UpdateThresholdsAndBroadcastAsync(AdminUpdatesThresholdsDto dto)
    {
        var currentLog = await cleanAirRepository.GetCurrentLogAsync();
        if (currentLog is null) throw new Exception("No current log for device");

        foreach (var t in dto.Thresholds)
        {
            var threshold = new DeviceThreshold
            {
                Metric = t.Metric,
                WarnMin = t.WarnMin,
                WarnMax = t.WarnMax,
                GoodMin = t.GoodMin,
                GoodMax = t.GoodMax,
            };

            await thresholdRepository.UpdateThresholdAsync(threshold);
        }

        var updateDeviceThresholds = new AdminUpdatesDeviceThresholdsDto
        {
            GoodMax = dto.Thresholds.FirstOrDefault(g => g.Metric == "airquality")?.GoodMax ?? 1200,
            WarnMax = dto.Thresholds.FirstOrDefault(w => w.Metric == "airquality")?.WarnMax ?? 2500,
        };
            
        await mqttPublisher.Publish(updateDeviceThresholds, StringConstants.UpdateDeviceThresholds);

        var updatedBroadcast = await GetThresholdsWithEvaluationsAsync();
        await connectionManager.BroadcastToTopic(StringConstants.Dashboard, updatedBroadcast);
    }

    public async Task<ThresholdsBroadcastDto> GetThresholdsWithEvaluationsAsync()
    {
        var thresholds = await thresholdRepository.GetAllAsync();
        var currentLog = await cleanAirRepository.GetCurrentLogAsync();
        if (currentLog is null) throw new Exception("No log found for evaluation");

        var evaluations = EvaluateThresholdsAgainstLog(currentLog, thresholds);

        return new ThresholdsBroadcastDto
        {
            UpdatedThresholds = thresholds.Select(t => new ThresholdDto
            {
                Metric = t.Metric,
                WarnMin = t.WarnMin,
                WarnMax = t.WarnMax,
                GoodMin = t.GoodMin,
                GoodMax = t.GoodMax
            }).ToList(),
            Evaluations = evaluations
        };
    }

    private List<ThresholdEvaluationResult> EvaluateThresholdsAgainstLog(
        Devicelog log,
        IEnumerable<DeviceThreshold> thresholds)
    {
        var results = new List<ThresholdEvaluationResult>();

        foreach (var threshold in thresholds)
        {
            var value = threshold.Metric switch
            {
                "temperature" => log.Temperature,
                "humidity" => log.Humidity,
                "pressure" => log.Pressure,
                "airquality" => (decimal)log.Airquality,
                _ => throw new Exception("Unknown metric: " + threshold.Metric)
            };

            results.Add(evaluator.Evaluate(threshold.Metric, value, threshold));
        }

        return results;
    }
}