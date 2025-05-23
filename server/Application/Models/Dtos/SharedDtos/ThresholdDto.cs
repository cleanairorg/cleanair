namespace Application.Models.Dtos.SharedDtos;

public class ThresholdDto
{
    public string Metric { get; set; } = null!;
    public decimal WarnMin { get; set; }
    public decimal GoodMin { get; set; }
    public decimal GoodMax { get; set; }
    public decimal WarnMax { get; set; }
}