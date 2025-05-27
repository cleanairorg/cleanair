using Application.Models.Dtos.BroadcastModels;
using WebSocketBoilerplate;

namespace Api.Websocket.Dtos;

/// <summary>
/// Server response with threshold data
/// </summary>
public class ServerRespondsWithThresholds : BaseDto
{
    public ThresholdsBroadcastDto ThresholdData { get; set; } = new();
}