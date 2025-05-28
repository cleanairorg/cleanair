namespace Application.Models.Dtos.RestDtos;

public class AggregatedLogDto
{
    public DateTime Date { get; set; }
    public decimal AvgTemperature { get; set; }
    public decimal AvgHumidity { get; set; }
    public decimal AvgPressure { get; set; }
    public double AvgAirQuality { get; set; }
}