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
        try
        {
            // Verificer admin rights
            var claims = securityService.VerifyJwtOrThrow(dto.Authorization);
            if (claims.Role != "admin")
            {
                socket.SendDto(new ServerSendsErrorMessage
                {
                    Message = "You are not authorized to update thresholds",
                    requestId = dto.requestId
                });
                return;
            }

            // ÆNDRING HER - brug dto.ThresholdData direkte
            if (dto.ThresholdData == null)
            {
                socket.SendDto(new ServerSendsErrorMessage
                {
                    Message = "No threshold data provided",
                    requestId = dto.requestId
                });
                return;
            }

            await thresholdService.UpdateThresholdsAndBroadcastAsync(dto.ThresholdData);
        
            // Send confirmation to sender
            socket.SendDto(new ServerConfirmsThresholdUpdate
            {
                Success = true,
                Message = "Thresholds updated successfully",
                requestId = dto.requestId
            });
        
            logger.LogInformation("[UpdateThresholdsHandler] Thresholds updated successfully via WebSocket");
        }
        catch (Exception ex)
        {
            logger.LogError("[UpdateThresholdsHandler] Unexpected error updating thresholds via WebSocket", ex);
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "An unexpected error occurred",
                requestId = dto.requestId
            });
        }
    }
}