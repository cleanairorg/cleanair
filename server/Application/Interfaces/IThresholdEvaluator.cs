using Application.Models.Dtos.SharedDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IThresholdEvaluator
{
    ThresholdEvaluationResult Evaluate(string metric, decimal value, DeviceThreshold threshold);
}