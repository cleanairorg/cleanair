using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class CleanAirController(
    ICleanAirService cleanAirService,
    IConnectionManager connectionManager,
    ISecurityService securityService) : ControllerBase
{
    public const string ControllerRoute = "api/";
    public const string GetLogsRoute = ControllerRoute + nameof(GetLogs);

    public const string AdminChangesPreferencesRoute = ControllerRoute + nameof(AdminChangesPreferences);
    
    public const string DeleteDataRoute = ControllerRoute + nameof(DeleteData);
    
    public const string GetMeasurementNowRoute = ControllerRoute + nameof(GetMeasurementNow);
    
    public const string GetLatestMeasurementRoute = ControllerRoute + nameof(GetLatestMeasurement);

    public const string AdminChangesDeviceIntervalRoute = ControllerRoute + nameof(AdminChangesDeviceInterval);

    [HttpGet]
    [Route(GetLogsRoute)]
    public async Task<ActionResult<IEnumerable<Devicelog>>> GetLogs([FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var feed = cleanAirService.GetDeviceFeed(claims);
        return Ok(feed);
    }

    [HttpPost]
    [Route(AdminChangesPreferencesRoute)]
    public async Task<ActionResult> AdminChangesPreferences([FromBody] AdminChangesPreferencesDto dto,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await cleanAirService.UpdateDeviceFeed(dto, claims);
        return Ok();
    }
    
    [HttpPost]
    [Route(AdminChangesDeviceIntervalRoute)]
    public async Task<ActionResult> AdminChangesDeviceInterval(
        [FromHeader] string authorization, 
        [FromBody] AdminChangesDeviceIntervalDto dto)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        if (claims.Role != "admin")
        {
            return Unauthorized("You are not authorized to change intervals");
        }

        await cleanAirService.UpdateDeviceIntervalAndBroadcast(dto);
        
        return Ok();
    }
    
    [HttpDelete]
    [Route(DeleteDataRoute)]
    public async Task<ActionResult> DeleteData([FromHeader]string authorization)
    {
        var jwt = securityService.VerifyJwtOrThrow(authorization);
        
        await cleanAirService.DeleteDataAndBroadcast(jwt);

        return Ok();
    }

    [HttpGet]
    [Route(GetMeasurementNowRoute)]
    public async Task<ActionResult> GetMeasurementNow([FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        if (claims.Role != "admin") {
            return Unauthorized("You are not authorized to access this route");
        }
        
        await cleanAirService.GetMeasurementNowAndBroadcast();
        
        return Ok();
    }
    
    [HttpGet]
    [Route(GetLatestMeasurementRoute)]
    public async Task<ActionResult<Devicelog>> GetLatestMeasurement()
    {
        
        var latestLog = cleanAirService.GetLatestDeviceLog();
        return Ok(latestLog);
        
    }
    

}