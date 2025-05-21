using Application.Models.Dtos.RestDtos;

namespace Application.Models.Dtos.BroadcastModels;

public class UpdatedThresholdsDto : ApplicationBaseDto
{
    public override string eventType { get; set; } = nameof(UpdatedThresholdsDto);
    public string DeviceId { get; set; } = null!;
    public List<ThresholdDto> UpdatedThresholds { get; set; } = new();
}