using WebSocketBoilerplate;

namespace Api.Websocket.Dtos;

/// <summary>
/// Server confirmation that thresholds were updated
/// </summary>
public class ServerConfirmsThresholdUpdate : BaseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}