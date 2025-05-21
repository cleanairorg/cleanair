namespace Application.Models.Dtos.RestDtos;

public class TimeRangeDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DeviceId { get; set; } = string.Empty;
}