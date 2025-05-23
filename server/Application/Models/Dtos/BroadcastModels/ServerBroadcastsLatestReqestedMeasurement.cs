using Core.Domain.Entities;

namespace Application.Models.Dtos.BroadcastModels;

public class ServerBroadcastsLatestReqestedMeasurement : ApplicationBaseDto
{
    public Devicelog LatestMeasurement { get; set; }
    
    public override string eventType { get; set; } = nameof(ServerBroadcastsLatestReqestedMeasurement);
}