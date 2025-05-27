using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using Api.Websocket.Dtos;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Fleck;
using WebSocketBoilerplate;

namespace Api.Websocket;

public class UpdateThresholdsHandler(
    IThresholdService thresholdService,
    ISecurityService securityService,
    ILoggingService logger) : BaseEventHandler<AdminUpdatesThresholdsRequestDto>
{
    public override async Task Handle(AdminUpdatesThresholdsRequestDto dto, IWebSocketConnection socket)
    {
        logger.LogInformation($"[UpdateThresholdsHandler] Received threshold update request with requestId: {dto.requestId}");
        
        try
        {
            // Log token validation attempt
            logger.LogInformation("[UpdateThresholdsHandler] Validating authorization token");
            
            // Verify admin rights
            var claims = securityService.VerifyJwtOrThrow(dto.Authorization);
            logger.LogInformation($"[UpdateThresholdsHandler] Token validated successfully for user with role: {claims.Role}");
            
            if (claims.Role != "admin")
            {
                logger.LogWarning($"[UpdateThresholdsHandler] Access denied - user role '{claims.Role}' is not admin");
                socket.SendDto(new ServerSendsErrorMessage
                {
                    Message = "You are not authorized to update thresholds",
                    requestId = dto.requestId
                });
                return;
            }

            // Validate threshold data
            if (dto.ThresholdData == null)
            {
                logger.LogWarning("[UpdateThresholdsHandler] No threshold data provided in request");
                socket.SendDto(new ServerSendsErrorMessage
                {
                    Message = "No threshold data provided",
                    requestId = dto.requestId
                });
                return;
            }

            
            if (dto.ThresholdData.Thresholds != null)
            {
                foreach (var threshold in dto.ThresholdData.Thresholds)
                {
                    logger.LogInformation($"[UpdateThresholdsHandler] Updating threshold for metric '{threshold.Metric}'");
                }
            }

            // Update thresholds
            await thresholdService.UpdateThresholdsAsync(dto.ThresholdData);
            logger.LogInformation("[UpdateThresholdsHandler] Thresholds updated and broadcast completed successfully");
        
            // Send confirmation to sender
            socket.SendDto(new ServerConfirmsThresholdUpdate
            {
                Success = true,
                Message = "Thresholds updated successfully",
                requestId = dto.requestId
            });
            
            logger.LogInformation($"[UpdateThresholdsHandler] Success response sent to client with requestId: {dto.requestId}");
        }
        catch (ValidationException ex)
        {
            logger.LogWarning($"[UpdateThresholdsHandler] Validation error: {ex.Message}");
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "Invalid data provided: " + ex.Message,
                requestId = dto.requestId
            });
        }
        catch (AuthenticationException ex)
        {
            logger.LogWarning($"[UpdateThresholdsHandler] Authentication failed: {ex.Message}");
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "Authentication failed",
                requestId = dto.requestId
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning($"[UpdateThresholdsHandler] Unauthorized access attempt: {ex.Message}");
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "Access denied",
                requestId = dto.requestId
            });
        }
        catch (Exception ex)
        {
            logger.LogError("[UpdateThresholdsHandler] Unexpected error during threshold update", ex);
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "An unexpected error occurred",
                requestId = dto.requestId
            });
        }
    }
}