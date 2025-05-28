using NUnit.Framework;
using Moq;
using Fleck;
using Application.Interfaces.Infrastructure.Logging;
using Api.Websocket;
using Api.Websocket.Dtos;
using System.Security.Authentication;
using System.ComponentModel.DataAnnotations;

namespace Api.Websocket.Tests;

[TestFixture]
public class WebSocketExceptionHandlerTests
{
    [Test]
    public void HandleException_DifferentExceptionTypes_SendsDifferentErrorMessages()
    {
        // Arrange
        var socketMock = new Mock<IWebSocketConnection>();
        var loggerMock = new Mock<ILoggingService>();
        var requestId = "test-123";
        var handlerName = "TestHandler";

        var authException = new AuthenticationException("Token expired");
        var validationException = new ValidationException("Invalid input");
        var unauthorizedException = new UnauthorizedAccessException("Access denied");
        var genericException = new Exception("Database error");

        // Act
        WebSocketExceptionHandler.HandleException(authException, socketMock.Object, requestId, loggerMock.Object, handlerName);
        WebSocketExceptionHandler.HandleException(validationException, socketMock.Object, requestId, loggerMock.Object, handlerName);
        WebSocketExceptionHandler.HandleException(unauthorizedException, socketMock.Object, requestId, loggerMock.Object, handlerName);
        WebSocketExceptionHandler.HandleException(genericException, socketMock.Object, requestId, loggerMock.Object, handlerName);

        // Assert
        socketMock.Verify(s => s.Send(It.Is<string>(msg => msg.Contains("Authentication failed"))), Times.Once);
        socketMock.Verify(s => s.Send(It.Is<string>(msg => msg.Contains("Invalid request"))), Times.Once);
        socketMock.Verify(s => s.Send(It.Is<string>(msg => msg.Contains("Access denied"))), Times.Once);
        socketMock.Verify(s => s.Send(It.Is<string>(msg => msg.Contains("An unexpected error occurred"))), Times.Once);
        
        // Verify total calls
        socketMock.Verify(s => s.Send(It.IsAny<string>()), Times.Exactly(4));
    }
}