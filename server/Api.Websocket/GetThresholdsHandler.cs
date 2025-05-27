using Application.Interfaces.Infrastructure.Logging;
using Application.Interfaces;
using Fleck;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using Api.Websocket.Dtos;
using WebSocketBoilerplate;

namespace Api.Websocket;

public class GetThresholdsHandler(
    IThresholdService thresholdService,
    ISecurityService securityService,
    ILoggingService logger) : BaseEventHandler<GetThresholdsRequestDto>
{
    public override async Task Handle(GetThresholdsRequestDto dto, IWebSocketConnection socket)
    {
        logger.LogInformation($"[GetThresholdsHandler] Received get thresholds request with requestId: {dto.requestId}");
        
        try
        {
            // Log token validation attempt
            logger.LogInformation("[GetThresholdsHandler] Validating authorization token");
            
            // Clean JWT token - fjern linjeskift og ekstra whitespace
            var cleanToken = dto.Authorization?.Replace("\n", "").Replace("\r", "").Trim();
            logger.LogInformation($"[GetThresholdsHandler] Token length after cleaning: {cleanToken?.Length ?? 0}");
            
            // Verificer authorization
            var claims = securityService.VerifyJwtOrThrow(cleanToken);
            logger.LogInformation($"[GetThresholdsHandler] Token validated successfully for user with role: {claims.Role}");
            
            // Retrieve thresholds
            logger.LogInformation("[GetThresholdsHandler] Retrieving thresholds with evaluations from service");
            var result = await thresholdService.GetThresholdsWithEvaluationsAsync();
            
            // Log result details
            var thresholdCount = result.UpdatedThresholds?.Count ?? 0;
            var evaluationCount = result.Evaluations?.Count ?? 0;
            logger.LogInformation($"[GetThresholdsHandler] Retrieved {thresholdCount} thresholds and {evaluationCount} evaluations");
            
            if (result.UpdatedThresholds != null)
            {
                foreach (var threshold in result.UpdatedThresholds)
                {
                    logger.LogInformation($"[GetThresholdsHandler] Threshold for '{threshold.Metric}': WarnMin={threshold.WarnMin}, GoodMin={threshold.GoodMin}, GoodMax={threshold.GoodMax}, WarnMax={threshold.WarnMax}");
                }
            }
            
            if (result.Evaluations != null)
            {
                foreach (var evaluation in result.Evaluations)
                {
                    logger.LogInformation($"[GetThresholdsHandler] Evaluation for '{evaluation.Metric}': Value={evaluation.Value}, State={evaluation.State}");
                }
            }
            
            // Send data directly back to requesting client
            socket.SendDto(new ServerRespondsWithThresholds
            {
                ThresholdData = result,
                requestId = dto.requestId
            });
            
            logger.LogInformation($"[GetThresholdsHandler] Threshold data sent successfully to client with requestId: {dto.requestId}");
        }
        catch (ValidationException ex)
        {
            logger.LogWarning($"[GetThresholdsHandler] Validation error: {ex.Message}");
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "Invalid request: " + ex.Message,
                requestId = dto.requestId
            });
        }
        catch (AuthenticationException ex)
        {
            logger.LogWarning($"[GetThresholdsHandler] Authentication failed: {ex.Message}");
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "Authentication failed",
                requestId = dto.requestId
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning($"[GetThresholdsHandler] Unauthorized access attempt: {ex.Message}");
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "Access denied",
                requestId = dto.requestId
            });
        }
        catch (Exception ex)
        {
            logger.LogError("[GetThresholdsHandler] Unexpected error retrieving thresholds", ex);
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "An unexpected error occurred",
                requestId = dto.requestId
            });
        }
    }
}