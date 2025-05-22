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
        try
        {
            logger.LogInformation($"[Controller]GetDailyAverages called with DTO: {JsonSerializer.Serialize(timeRangeDto)}");
            
            var result = weatherStationService.GetDailyAverages(timeRangeDto);
            
            logger.LogInformation($"[Controller]GetDailyAverages succeeded. Returned {result.Count} entries.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError($"[Controller] Error in GetDailyAverages with DTO: {JsonSerializer.Serialize(timeRangeDto)}", ex);
            return StatusCode(500, "An error occurred while retrieving daily averages.");
        }
    }


    [HttpPost("api/GetLogsForToday")]
    public ActionResult<List<Devicelog>> GetLogsForToday([FromBody] TimeRangeDto timeRangeDto)
    {
        try
        {
            logger.LogInformation($"[Controller]GetLogsForToday called with DTO: {JsonSerializer.Serialize(timeRangeDto)}");

            var logs = weatherStationService.GetLogsForToday(timeRangeDto);

            logger.LogInformation($"[Controller]GetLogsForToday succeeded. Returned {logs.Count} logs.");
            return Ok(logs);
        }
        catch (Exception ex)
        {
            logger.LogError($"[Controller]Error in GetLogsForToday with DTO: {JsonSerializer.Serialize(timeRangeDto)}", ex);
            return StatusCode(500, "An error occurred while retrieving today's logs.");
        }
    }


    
    



}