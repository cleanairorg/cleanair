using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Application.Models.Dtos.MqttSubscriptionDto;

public class CollectDataDto
{
    
    [Required] [MinLength(1)]
    public String DeviceId { get; set; } = null!;

    [Required]
    public float Temperature { get; set; }
    
    [Required]
    public float Humidity { get; set; }
    
    [Required]
    public float Pressure { get; set; }
    
    [Required]
    public float AirQuality { get; set; }
}