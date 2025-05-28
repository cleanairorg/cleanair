using NUnit.Framework;
using Moq;
using Fleck;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Api.Websocket;
using Api.Websocket.Dtos;
using Application.Models;

namespace Api.Websocket.Tests;

[TestFixture]
public class UpdateThresholdsHandlerTests
{
    [Test]
    public async Task Handle_UserRole_AccessDenied()
    {
        // Arrange
        var thresholdServiceMock = new Mock<IThresholdService>();
        var securityServiceMock = new Mock<ISecurityService>();
        var loggerMock = new Mock<ILoggingService>();
        var socketMock = new Mock<IWebSocketConnection>();
        
        var handler = new UpdateThresholdsHandler(
            thresholdServiceMock.Object,
            securityServiceMock.Object,
            loggerMock.Object);

        var userDto = new AdminUpdatesThresholdsRequestDto
        {
            requestId = "user-request",
            Authorization = "Bearer user-token"
        };

        var userClaims = new JwtClaims
        {
            Role = "user", // Not admin!
            Id = "user123",
            Email = "user@test.com",
            Exp = DateTime.UtcNow.ToString()
        };

        securityServiceMock.Setup(s => s.VerifyJwtOrThrow("Bearer user-token"))
            .Returns(userClaims);

        // Act
        await handler.Handle(userDto, socketMock.Object);

        // Assert
        thresholdServiceMock.VerifyNoOtherCalls();
    }
}