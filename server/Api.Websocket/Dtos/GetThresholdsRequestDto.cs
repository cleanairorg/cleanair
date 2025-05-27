using WebSocketBoilerplate;

namespace Api.Websocket.Dtos;

/// <summary>
/// Client request to get current thresholds
/// Frontend should send:
/// {
///   "eventType": "GetThresholdsRequestDto", 
///   "authorization": "Bearer token...",
///   "requestId": "unique-id"
/// }
/// </summary>
public class GetThresholdsRequestDto : BaseDto
{
    public string Authorization { get; set; } = string.Empty;
}