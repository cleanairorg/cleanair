namespace Application.Models.Dtos.RestDtos;

public class AdminUpdatesThresholdsDto
{
    public string DeviceId { get; set; } = null!;
    public List<ThresholdDto> Thresholds { get; set; } = new();
}