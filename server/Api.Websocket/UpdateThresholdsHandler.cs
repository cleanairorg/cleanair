using Application.Interfaces.Infrastructure.Logging;
using Application.Interfaces;
using Fleck;
using Api.Websocket.Dtos;
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
            // Authentication logic
            logger.LogInformation("[UpdateThresholdsHandler] Validating authorization token");
            var claims = securityService.VerifyJwtOrThrow(dto.Authorization);
            logger.LogInformation($"[UpdateThresholdsHandler] Token validated successfully for user with role: {claims.Role}");
            
            // Authorization check
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

            // Validation
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

            // update thresholds
            await thresholdService.UpdateThresholdsAsync(dto.ThresholdData);
            logger.LogInformation("[UpdateThresholdsHandler] Thresholds updated and broadcast completed successfully");
        
            // Send success response
            socket.SendDto(new ServerConfirmsThresholdUpdate
            {
                Success = true,
                Message = "Thresholds updated successfully",
                requestId = dto.requestId
            });
            
            logger.LogInformation($"[UpdateThresholdsHandler] Success response sent to client with requestId: {dto.requestId}");
        }
        catch (Exception ex)
        {
            WebSocketExceptionHandler.HandleException(ex, socket, dto.requestId, logger, "UpdateThresholdsHandler");
        }
    }
}