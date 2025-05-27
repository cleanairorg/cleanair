using System.Text.Json;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class CleanAirController(
    ICleanAirService cleanAirService,
    IConnectionManager connectionManager,
    ISecurityService securityService, 
    ILoggingService logger) : ControllerBase
{
    public const string ControllerRoute = "api/";
    public const string GetLogsRoute = ControllerRoute + nameof(GetLogs);

    public const string AdminChangesPreferencesRoute = ControllerRoute + nameof(AdminChangesPreferences);
    
    public const string DeleteDataRoute = ControllerRoute + nameof(DeleteData);
    
    public const string GetMeasurementNowRoute = ControllerRoute + nameof(GetMeasurementNow);
    
    public const string GetLatestMeasurementRoute = ControllerRoute + nameof(GetLatestMeasurement);
    
    public const string GetDailyAveragesRoute = ControllerRoute + nameof(GetDailyAverages);
    
    public const string GetLogsForTodayRoute = ControllerRoute + nameof(GetLogsForToday);

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
    public async Task<ActionResult> DeleteData([FromHeader] string authorization)
    {
        logger.LogInformation("[CleanAirController] DeleteData endpoint called");

        var claims = securityService.VerifyJwtOrThrow(authorization);
        logger.LogInformation("[CleanAirController] JWT verified.");

        if (claims.Role != "admin")
        {
            logger.LogWarning(
                "[CleanAirController] Unauthorized access to DeleteData endpoint called (non admin called deletion)");
            return Unauthorized("You are not authorized to delete data");
        }

        try
        {
            await cleanAirService.DeleteDataAndBroadcast(claims);
            logger.LogInformation("[CleanAirController] Deleted data");
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError("[CleanAirController] Failed to delete data.");
            return StatusCode(500, "Internal server error");
        }
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
    
    [HttpPost]
    [Route(GetDailyAveragesRoute)]
    public ActionResult<List<Devicelog>> GetDailyAverages(
        [FromBody] TimeRangeDto dto,
        [FromHeader(Name = "Authorization")] string authorization)
    {
        try
        {
            var claims = securityService.VerifyJwtOrThrow(authorization);
            logger.LogInformation($"[Controller] GetDailyAverages called with DTO: {JsonSerializer.Serialize(dto)}");
            var result = cleanAirService.GetDailyAverages(dto);
            logger.LogInformation($"[Controller] GetDailyAverages succeeded. Returned {result.Count} entries.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError($"[Controller] Error in GetDailyAverages: {JsonSerializer.Serialize(dto)}", ex);
            return StatusCode(500, "An error occurred while retrieving daily averages.");
        }
    }

    

    [HttpPost]
    [Route(GetLogsForTodayRoute)]
    public ActionResult<List<Devicelog>> GetLogsForToday(
        [FromBody] TimeRangeDto timeRangeDto,
        [FromHeader(Name = "Authorization")] string authorization)
    {
        try
        {
            var claims = securityService.VerifyJwtOrThrow(authorization);
            logger.LogInformation($"[Controller] GetLogsForToday called with DTO: {JsonSerializer.Serialize(timeRangeDto)}");
            var logs = cleanAirService.GetLogsForToday(timeRangeDto);
            logger.LogInformation($"[Controller] GetLogsForToday succeeded. Returned {logs.Count} logs.");
            return Ok(logs);
        }
        catch (Exception ex)
        {
            logger.LogError($"[Controller] Error in GetLogsForToday with DTO: {JsonSerializer.Serialize(timeRangeDto)}", ex);
            return StatusCode(500, "An error occurred while retrieving today's logs.");
        }
    }

    
}