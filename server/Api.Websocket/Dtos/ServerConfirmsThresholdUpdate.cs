using WebSocketBoilerplate;

namespace Api.Websocket.Dtos;

/*
/ Server confirmation that thresholds were updated
*/
public class ServerConfirmsThresholdUpdate : BaseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}