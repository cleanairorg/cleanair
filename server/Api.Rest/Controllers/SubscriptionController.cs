using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class SubscriptionController(
    ISecurityService securityService,
    IConnectionManager connectionManager,
    IWebsocketSubscriptionService websocketSubscriptionService) : ControllerBase
{
    public const string ControllerRoute = "api/";
    
    public const string SubscriptionRoute = ControllerRoute + nameof(Subscribe);

    public const string UnsubscribeRoute = ControllerRoute + nameof(Unsubscribe);

    public const string ExampleBroadcastRoute = ControllerRoute + nameof(ExampleBroadcast);

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

    [HttpPost]
    [Route(ExampleBroadcastRoute)]
    public async Task<ActionResult> ExampleBroadcast([FromBody] ExampleBroadcastDto dto)
    {
        await connectionManager.BroadcastToTopic("ExampleTopic", dto);
        return Ok();
    }
}

public class ExampleBroadcastDto
{
    public string eventType { get; set; } = nameof(ExampleBroadcastDto);
    public string Message { get; set; }
}