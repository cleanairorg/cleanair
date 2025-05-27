using Application.Models.Dtos.RestDtos;
using WebSocketBoilerplate;

namespace Api.Websocket.Dtos;

/// <summary>
/// Client request to update thresholds
/// Frontend should send:
/// {
///   "eventType": "AdminUpdatesThresholdsRequestDto",
///   "authorization": "Bearer token...",
///   "thresholdData": { ... },
///   "requestId": "unique-id"
/// }
/// </summary>
public class AdminUpdatesThresholdsRequestDto : BaseDto
{
    public string Authorization { get; set; } = string.Empty;
    public AdminUpdatesThresholdsDto ThresholdData { get; set; } = new();
}