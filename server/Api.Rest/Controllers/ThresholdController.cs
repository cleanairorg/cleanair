using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Application.Models.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class ThresholdController(
    IThresholdService thresholdService, 
    ISecurityService securityService) : ControllerBase
{
    public const string ControllerRoute = "api/";
    public const string UpdateThresholdsRoute = ControllerRoute + nameof(UpdateThresholds);
    public const string GetThresholdsRoute = ControllerRoute + nameof(GetThresholds);
    
    [HttpPost]
    [Route(UpdateThresholdsRoute)]
    public async Task<IActionResult> UpdateThresholds(
        [FromBody] AdminUpdatesThresholdsDto updateThresholdsDto, 
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);

        /*if (claims.Role != Constants.AdminRole)
        {
            return Forbid();
        }*/
        
        await thresholdService.UpdateThresholdsAndBroadcastAsync(updateThresholdsDto);
        return Ok();
    }

    [HttpGet]
    [Route(GetThresholdsRoute)]
    public async Task<ActionResult<ThresholdsBroadcastDto>> GetThresholds(
        [FromQuery] string deviceId,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
    
        var result = await thresholdService.GetThresholdsWithEvaluationAsync(deviceId);
        return Ok(result);
    }
}