using Application.Models.Enums;

namespace Application.Models.Dtos.SharedDtos;

public class ThresholdEvaluationResult
{
    public string Metric { get; set; } = null!;
    public decimal Value { get; set; }
    public ThresholdStates State { get; set; }
}