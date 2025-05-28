using System.Text.Json;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using FeatureHubSDK;
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
        var feed =  cleanAirService.GetDeviceFeed(claims);
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
        try
        {
            logger.LogInformation("[CleanAirController] AdminChangesDeviceInterval endpoint called");
            var claims = securityService.VerifyJwtOrThrow(authorization);
            logger.LogInformation($"[CleanAirController] JWT verified. User Role: {claims.Role}");
            
            if (claims.Role != "admin")
            {
                logger.LogWarning("[CleanAirController] Unauthorized User access attempt");
                return Unauthorized("You are not authorized to change intervals");
            }

            logger.LogInformation("[CleanAirController] Authorized User access attempt");
            await cleanAirService.UpdateDeviceIntervalAndBroadcast(dto);
            
            logger.LogInformation("[CleanAirController] Device interval changed");
            return Ok();
        }
        catch (Exception)
        {
            logger.LogError("[CleanAirController] Error occurred in AdminChangesDeviceInterval");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete]
    [Route(DeleteDataRoute)]
    public async Task<ActionResult> DeleteData([FromHeader] string authorization)
    {
        logger.LogInformation("[CleanAirController] DeleteData endpoint called");

        var claims = securityService.VerifyJwtOrThrow(authorization);
        logger.LogInformation($"[CleanAirController] JWT verified. User Role: {claims.Role}");

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
            logger.LogError("[CleanAirController] Failed to delete data.", ex);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    [Route(GetMeasurementNowRoute)]
    public async Task<ActionResult> GetMeasurementNow([FromHeader] string authorization)
    {
        try
        {
            logger.LogInformation("[CleanAirController] GetMeasurementNow endpoint called");
            var claims = securityService.VerifyJwtOrThrow(authorization);
            if (claims.Role != "admin")
            {
                logger.LogWarning("[CleanAirController] Unauthorized User access attempt");
                return Unauthorized("You are not authorized to access this route");
            }
            logger.LogWarning("[CleanAirController] Authorized User access attempt, triggering GetMeasurementNow");
            await cleanAirService.GetMeasurementNowAndBroadcast();
            logger.LogWarning("[CleanAirController] Measurement successfully triggered");
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError("[CleanAirController] Error occurred in GetMeasurementNow", ex);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpGet]
    [Route(GetLatestMeasurementRoute)]
    public async Task<ActionResult<Devicelog>> GetLatestMeasurement()
    {
        try
        {
            logger.LogInformation("[CleanAirController] GetLatestMeasurement endpoint called");
            var latestLog = cleanAirService.GetLatestDeviceLog();

            if (latestLog == null)
            {
                logger.LogWarning("[CleanAirController] LatestLog is null");
                return NotFound("No latest log found");
            }

            logger.LogInformation($"[CleanAirController] Latest log found and retrieved successfully, LogID: {latestLog.Id}");
            return Ok(latestLog);
        }
        catch (Exception ex)
        {
            logger.LogError("[CleanAirController] Error occurred in GetLatestMeasurement", ex);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost]
    [Route(GetDailyAveragesRoute)]
    public ActionResult<List<Devicelog>> GetDailyAverages(
        [FromBody] TimeRangeDto dto,
        [FromHeader(Name = "Authorization")] string authorization)
    {
        try
        {
            securityService.VerifyJwtOrThrow(authorization);
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
            securityService.VerifyJwtOrThrow(authorization);
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