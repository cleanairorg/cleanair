using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Models.Dtos.SharedDtos;
using Application.Models.Enums;
using Core.Domain.Entities;

namespace Application.Services;

public class ThresholdEvaluator : IThresholdEvaluator
{
    public ThresholdEvaluationResult Evaluate(string metric, decimal value, DeviceThreshold threshold)
    {
        var (warnMin, goodMin, goodMax, warnMax) = (threshold.WarnMin, threshold.GoodMin, threshold.GoodMax, threshold.WarnMax);

        ThresholdStates state;
        if (value < warnMin) state = ThresholdStates.CriticalLow;
        else if (value < goodMin) state = ThresholdStates.WarningLow;
        else if (value <= goodMax) state = ThresholdStates.Good;
        else if (value <= warnMax) state = ThresholdStates.WarningHigh;
        else state = ThresholdStates.CriticalHigh;

        return new ThresholdEvaluationResult
        {
            Metric = metric,
            Value = value,
            State = state
        };
    }
}