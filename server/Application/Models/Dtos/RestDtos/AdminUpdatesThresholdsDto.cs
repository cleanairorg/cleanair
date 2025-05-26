using Application.Models.Dtos.SharedDtos;

namespace Application.Models.Dtos.RestDtos;

public class AdminUpdatesThresholdsDto
{
    public List<ThresholdDto> Thresholds { get; set; } = new();
}