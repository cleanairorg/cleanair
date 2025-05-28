using Application.Models.Dtos.BroadcastModels;
using WebSocketBoilerplate;

namespace Api.Websocket.Dtos;

/*
/// Server response with threshold data
*/
public class ServerRespondsWithThresholds : BaseDto
{
    public ThresholdsBroadcastDto ThresholdData { get; set; } = new();
}