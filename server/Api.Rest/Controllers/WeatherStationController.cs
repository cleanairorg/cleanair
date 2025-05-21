using System.Text.Json;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class WeatherStationController(
    IWeatherStationService weatherStationService,
    IConnectionManager connectionManager,
    ISecurityService securityService, 
    ILoggingService logger) : ControllerBase
{
    public const string ControllerRoute = "api/";
    public const string GetLogsRoute = ControllerRoute + nameof(GetLogs);

    public const string AdminChangesPreferencesRoute = ControllerRoute + nameof(AdminChangesPreferences);

    public const string DeleteDataRoute = ControllerRoute + nameof(DeleteData);
    
    public const string GetMeasurementNowRoute = ControllerRoute + nameof(GetMeasurementNow);

    [HttpGet]
    [Route(GetLogsRoute)]
    public async Task<ActionResult<IEnumerable<Devicelog>>> GetLogs([FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var feed = weatherStationService.GetDeviceFeed(claims);
        return Ok(feed);
    }

    [HttpPost]
    [Route(AdminChangesPreferencesRoute)]
    public async Task<ActionResult> AdminChangesPreferences([FromBody] AdminChangesPreferencesDto dto,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await weatherStationService.UpdateDeviceFeed(dto, claims);
        return Ok();
    }

    [HttpDelete]
    [Route(DeleteDataRoute)]
    public async Task<ActionResult> DeleteData([FromHeader] string authorization)
    {
        var jwt = securityService.VerifyJwtOrThrow(authorization);

        await weatherStationService.DeleteDataAndBroadcast(jwt);

        return Ok();
    }

    [HttpGet]
    [Route(GetMeasurementNowRoute)]
    public async Task<ActionResult> GetMeasurementNow()
    {
        //securityService.VerifyJwtOrThrow(authorization);
        
        await weatherStationService.GetMeasurementNowAndBroadcast();
        
        return Ok();
    }


    [HttpPost("api/GetDailyAverages")]
    public ActionResult<List<AggregatedLogDto>> GetDailyAverages([FromBody] TimeRangeDto timeRangeDto)
    {
        /*Console.WriteLine("------------");
        Console.WriteLine($"[Console] GetDailyAverages DTO: {JsonSerializer.Serialize(timeRangeDto)}");
        Console.WriteLine("------------");*/
        logger.LogInformation($"[Console] GetDailyAverages DTO: {timeRangeDto}");

        var result = weatherStationService.GetDailyAverages(timeRangeDto);
        
        return Ok(result);
    }
    
    
    [HttpPost("api/GetLogsForToday")]
    public ActionResult<List<Devicelog>> GetLogsForToday([FromBody] TimeRangeDto timeRangeDto)
    {
        var logs = weatherStationService.GetLogsForToday(timeRangeDto);
        return Ok(logs);
    }
    
    



}