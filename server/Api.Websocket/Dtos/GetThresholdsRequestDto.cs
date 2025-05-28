using WebSocketBoilerplate;

namespace Api.Websocket.Dtos;

/*
/ Client request to get current thresholds
*/
public class GetThresholdsRequestDto : BaseDto
{
    public string Authorization { get; set; } = string.Empty;
}