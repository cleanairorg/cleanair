using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
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
        
        await thresholdService.UpdateThresholdAndBroadcastAsync(updateThresholdsDto);
        return Ok();
    }
}