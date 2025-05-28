using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.SharedDtos;
using Core.Domain.Entities;

namespace Application.Services;

public class ThresholdService(
    IDeviceThresholdRepository thresholdRepository,
    ICleanAirRepository cleanAirRepository,
    IMqttPublisher mqttPublisher,
    IThresholdEvaluator evaluator,
    ILoggingService logger) : IThresholdService
{
    
    public async Task UpdateThresholdsAsync(AdminUpdatesThresholdsDto adminUpdatesThresholdsDto)
    {
        logger.LogInformation("[ThresholdService] Starting threshold update process, UpdateThresholdsAsync");
        
        try
        {
            // Get current log for evaluation
            logger.LogInformation("[ThresholdService] Retrieving current device log for evaluation");
            var currentLog = await cleanAirRepository.GetCurrentLogAsync();
            if (currentLog is null) 
            {
                logger.LogError("[ThresholdService] No current log found for device - cannot proceed with threshold update");
                throw new ArgumentNullException(nameof(currentLog), "No current log for device");
            }

            if (adminUpdatesThresholdsDto.Thresholds != null)
            {
                foreach (var t in adminUpdatesThresholdsDto.Thresholds)
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
                    logger.LogInformation($"[ThresholdService] Successfully updated threshold for metric '{t.Metric}' in database");
                }
            }

            var updateDeviceThresholds = new AdminUpdatesDeviceThresholdsDto
            {
                GoodMax = adminUpdatesThresholdsDto.Thresholds?.FirstOrDefault(g => g.Metric == "airquality")?.GoodMax ?? 1200,
                WarnMax = adminUpdatesThresholdsDto.Thresholds?.FirstOrDefault(w => w.Metric == "airquality")?.WarnMax ?? 2500,
            };
            
            await mqttPublisher.Publish(updateDeviceThresholds, StringConstants.UpdateDeviceThresholds);
            logger.LogInformation("[ThresholdService] MQTT threshold update published successfully");
        }
        catch (Exception ex)
        {
            logger.LogError("[ThresholdService] Error during threshold update process", ex);
            throw;
        }
    }

    public async Task<ThresholdsBroadcastDto> GetThresholdsWithEvaluationsAsync()
    {
        logger.LogInformation("[ThresholdService] Starting threshold retrieval with evaluations, GetThresholdsWithEvaluationsAsync");
        
        try
        {
            logger.LogInformation("[ThresholdService] Retrieving all thresholds from repository");
            var thresholds = await thresholdRepository.GetAllAsync();
            
            logger.LogInformation("[ThresholdService] Retrieving current device log for threshold evaluations");
            var currentLog = await cleanAirRepository.GetCurrentLogAsync();
            if (currentLog is null) 
            {
                logger.LogError("[ThresholdService] No current log found for evaluation");
                throw new ArgumentNullException(nameof(currentLog), "No log found for evaluation");
            }
            
            logger.LogInformation($"[ThresholdService] Using log ID: {currentLog.Id} with values - Temperature: {currentLog.Temperature}, Humidity: {currentLog.Humidity}, Pressure: {currentLog.Pressure}, AirQuality: {currentLog.Airquality}");

            // Evaluate thresholds against current log
            logger.LogInformation("[ThresholdService] Evaluating thresholds against current sensor values");
            var evaluations = EvaluateThresholdsAgainstLog(currentLog, thresholds);
            logger.LogInformation($"[ThresholdService] Completed evaluation of {evaluations.Count} threshold evaluations");

            // Log evaluation results
            foreach (var evaluation in evaluations)
            {
                logger.LogInformation($"[ThresholdService] Evaluation result - Metric: '{evaluation.Metric}', Value: {evaluation.Value}, State: {evaluation.State}");
            }

            var result = new ThresholdsBroadcastDto
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

            logger.LogInformation($"[ThresholdService] Successfully prepared broadcast DTO with {result.UpdatedThresholds.Count} thresholds and {result.Evaluations.Count} evaluations");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError("[ThresholdService] Error during threshold retrieval with evaluations", ex);
            throw;
        }
    }

    private List<ThresholdEvaluationResult> EvaluateThresholdsAgainstLog(
        Devicelog log,
        IEnumerable<DeviceThreshold> thresholds)
    {
        logger.LogInformation("[ThresholdService] Starting threshold evaluation against sensor log, EvaluateThresholdsAgainstLog");
        var results = new List<ThresholdEvaluationResult>();

        foreach (var threshold in thresholds)
        {
            try
            {
                logger.LogInformation($"[ThresholdService] Evaluating threshold for metric '{threshold.Metric}'");
                
                var value = threshold.Metric switch
                {
                    "temperature" => log.Temperature,
                    "humidity" => log.Humidity,
                    "pressure" => log.Pressure,
                    "airquality" => (decimal)log.Airquality,
                    _ => throw new ArgumentException($"Unknown metric: {threshold.Metric}", nameof(threshold.Metric))
                };
                
                var evaluation = evaluator.Evaluate(threshold.Metric, value, threshold);
                results.Add(evaluation);
                
                logger.LogInformation($"[ThresholdService] Evaluation completed for '{threshold.Metric}' - Result: {evaluation.State}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[ThresholdService] Error evaluating threshold for metric '{threshold.Metric}'", ex);
                throw;
            }
        }

        logger.LogInformation($"[ThresholdService] Completed evaluation of all {results.Count} thresholds");
        return results;
    }
}