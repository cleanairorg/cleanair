using Application.Interfaces.Infrastructure.Logging;
using Fleck;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using Api.Websocket.Dtos;
using WebSocketBoilerplate;

namespace Api.Websocket;

/// <summary>
/// Simple exception handler for all websocket handlers
/// </summary>
public static class WebSocketExceptionHandler
{
    public static void HandleException(
        Exception ex, 
        IWebSocketConnection socket, 
        string requestId, 
        ILoggingService logger, 
        string handlerName)
    {
        switch (ex)
        {
            case ValidationException validationEx:
                logger.LogWarning($"[{handlerName}] Validation error: {validationEx.Message}");
                SendError(socket, requestId, "Invalid request: " + validationEx.Message);
                break;
                
            case AuthenticationException authEx:
                logger.LogWarning($"[{handlerName}] Authentication failed: {authEx.Message}");
                SendError(socket, requestId, "Authentication failed");
                break;
                
            case UnauthorizedAccessException unAuthEx:
                logger.LogWarning($"[{handlerName}] Unauthorized access attempt: {unAuthEx.Message}");
                SendError(socket, requestId, "Access denied");
                break;
                
            default:
                logger.LogError($"[{handlerName}] Unexpected error", ex);
                SendError(socket, requestId, "An unexpected error occurred");
                break;
        }
    }

    private static void SendError(IWebSocketConnection socket, string requestId, string message)
    {
        socket.SendDto(new ServerSendsErrorMessage
        {
            Message = message,
            requestId = requestId
        });
    }
}