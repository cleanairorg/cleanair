using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.SharedDtos;

namespace Application.Models.Dtos.BroadcastModels;

public class ThresholdsBroadcastDto : ApplicationBaseDto
{
    public override string eventType { get; set; } = nameof(ThresholdsBroadcastDto);
    public List<ThresholdDto> UpdatedThresholds { get; set; } = new();
    public List<ThresholdEvaluationResult> Evaluations { get; set; } = new();
}
