namespace Application.Models.Dtos.BroadcastModels;

public class ServerBroadcastsIntervalChange : ApplicationBaseDto
{
    public int Interval { get; set; }
    
    public override string eventType { get; set; } = nameof(ServerBroadcastsIntervalChange);
}