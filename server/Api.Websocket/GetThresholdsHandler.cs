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
        try
        {
            // Verificer authorization
            var claims = securityService.VerifyJwtOrThrow(dto.Authorization);
            
            // Brug din eksisterende service
            var result = await thresholdService.GetThresholdsWithEvaluationsAsync();
            
            // Send data directly back to requesting client
            socket.SendDto(new ServerRespondsWithThresholds
            {
                ThresholdData = result,
                requestId = dto.requestId
            });
            
            logger.LogInformation("[GetThresholdsHandler] Thresholds data sent to client via WebSocket");
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("[GetThresholdsHandler] Validation error getting thresholds");
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "Invalid request",
                requestId = dto.requestId
            });
        }
        catch (AuthenticationException ex)
        {
            logger.LogWarning("[GetThresholdsHandler] Authentication error getting thresholds");
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "Authentication failed",
                requestId = dto.requestId
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("[GetThresholdsHandler] Authorization error getting thresholds");
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "Access denied",
                requestId = dto.requestId
            });
        }
        catch (Exception ex)
        {
            logger.LogError("[GetThresholdsHandler] Unexpected error getting thresholds via WebSocket", ex);
            socket.SendDto(new ServerSendsErrorMessage
            {
                Message = "An unexpected error occurred",
                requestId = dto.requestId
            });
        }
    }
}