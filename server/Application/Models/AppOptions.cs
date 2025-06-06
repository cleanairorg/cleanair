using System.ComponentModel.DataAnnotations;

// ReSharper disable InconsistentNaming

namespace Application.Models;

public sealed class AppOptions
{
    [Required] public string JwtSecret { get; set; } = string.Empty!;
    [Required] public string DbConnectionString { get; set; } = string.Empty!;
    
    public string SeqUrl { get; set; } = "http://localhost:5341";
    public bool Seed { get; set; } = true;
    public int PORT { get; set; } = 8080;
    public int WS_PORT { get; set; } = 8181;
    public int REST_PORT { get; set; } = 5000;
    public string MQTT_BROKER_HOST { get; set; } = null!;
    public string MQTT_USERNAME { get; set; } = null!;
    public string MQTT_PASSWORD { get; set; } = null!;
    public string FEATUREHUB_API_KEY { get; set; } = null!;
    public string FEATUREHUB_URL { get; set; } = null!;
}