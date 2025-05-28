using Application.Models.Dtos.RestDtos;
using WebSocketBoilerplate;

namespace Api.Websocket.Dtos;

/*
/ Client request to update thresholds
*/
public class AdminUpdatesThresholdsRequestDto : BaseDto
{
    public string Authorization { get; set; } = string.Empty;
    public AdminUpdatesThresholdsDto ThresholdData { get; set; } = new();
}