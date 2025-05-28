using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api")] 
public class SubscriptionController(
    ISecurityService securityService,
    IWebsocketSubscriptionService websocketSubscriptionService) : ControllerBase
{
    public const string SubscriptionRoute = "Subscribe";
    public const string UnsubscribeRoute = "Unsubscribe";

    [HttpPost]
    [Route(SubscriptionRoute)]  
    public async Task<ActionResult> Subscribe([FromHeader] string authorization, [FromBody] ChangeSubscriptionDto dto)
    {
        securityService.VerifyJwtOrThrow(authorization);
        await websocketSubscriptionService.SubscribeToTopic(dto.ClientId, dto.TopicIds);
        return Ok();
    }

    [HttpPost]
    [Route(UnsubscribeRoute)]  
    public async Task<ActionResult> Unsubscribe([FromHeader] string authorization, [FromBody] ChangeSubscriptionDto dto)
    {
        securityService.VerifyJwtOrThrow(authorization);
        await websocketSubscriptionService.UnsubscribeFromTopic(dto.ClientId, dto.TopicIds);
        return Ok();
    }

}
