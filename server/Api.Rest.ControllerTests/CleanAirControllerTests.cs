using Api.Rest.Controllers;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Application.Models;
using Core.Domain.Entities;
using FeatureHubSDK;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Api.Rest.ControllerTests;

[TestFixture]
public class CleanAirControllerTests
{
    private Mock<ICleanAirService> _cleanAirServiceMock = null!;
    private Mock<IConnectionManager> _connectionManagerMock = null!;
    private Mock<ISecurityService> _securityServiceMock = null!;
    private Mock<ILoggingService> _loggerMock = null!;
    private CleanAirController _controller = null!;
    //private Mock<IFeatureHubRepository> _featureHubRepoMock;

    [SetUp]
    public void SetUp()
    {
        _cleanAirServiceMock = new Mock<ICleanAirService>();
        _connectionManagerMock = new Mock<IConnectionManager>();
        _securityServiceMock = new Mock<ISecurityService>();
        _loggerMock = new Mock<ILoggingService>();
        //_featureHubRepoMock = new Mock<IFeatureHubRepository>();

        _controller = new CleanAirController(
            _cleanAirServiceMock.Object,
            _connectionManagerMock.Object,
            _securityServiceMock.Object,
            _loggerMock.Object
        );
    }


    private static JwtClaims CreateJwt(string role = "user") => new()
    {
        Id = "mock-user-id",
        Email = "test@example.com",
        Role = role,
        Exp = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString()
    };

    [Test]
    public async Task GetMeasurementNow_AdminRole_ShouldReturnOk()
    {
        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow("valid-token"))
            .Returns(CreateJwt("admin"));

        _cleanAirServiceMock
            .Setup(s => s.GetMeasurementNowAndBroadcast())
            .Returns(Task.CompletedTask);

        var result = await _controller.GetMeasurementNow("valid-token");

        Assert.That(result, Is.InstanceOf<OkResult>());
        _cleanAirServiceMock.Verify(s => s.GetMeasurementNowAndBroadcast(), Times.Once);
    }

    [Test]
    public async Task GetMeasurementNow_NotAdmin_ShouldReturnUnauthorized()
    {
        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow("user-token"))
            .Returns(CreateJwt("user"));

        var result = await _controller.GetMeasurementNow("user-token");

        var unauthorized = result as UnauthorizedObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(unauthorized, Is.Not.Null);
            Assert.That(unauthorized!.StatusCode, Is.EqualTo(401));
        });
        Assert.That(unauthorized.Value, Is.EqualTo("You are not authorized to access this route"));
    }

    [Test]
    public void GetLogsForToday_ValidRequest_ShouldReturnLogs()
    {
        var dto = new TimeRangeDto
        {
            StartDate = DateTime.UtcNow.AddHours(-2),
            EndDate = DateTime.UtcNow
        };
        var logs = new List<Devicelog> { new() { Id = "log1" } };

        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow(It.IsAny<string>()))
            .Returns(CreateJwt());

        _cleanAirServiceMock
            .Setup(s => s.GetLogsForToday(dto))
            .Returns(logs);

        var result = _controller.GetLogsForToday(dto, "auth");

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(logs));
    }

    [Test]
    public void GetLogsForToday_ServiceThrows_ShouldReturn500()
    {
        var dto = new TimeRangeDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow
        };

        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow(It.IsAny<string>()))
            .Returns(CreateJwt());

        _cleanAirServiceMock
            .Setup(s => s.GetLogsForToday(dto))
            .Throws(new Exception("fail"));

        var result = _controller.GetLogsForToday(dto, "auth");

        var error = result.Result as ObjectResult;
        
        Assert.Multiple(() =>
        {
            Assert.That(error, Is.Not.Null);
            Assert.That(error!.StatusCode, Is.EqualTo(500));
            Assert.That(error.Value, Is.EqualTo("An error occurred while retrieving today's logs."));
        });

    }

    [Test]
    public void GetDailyAverages_ValidRequest_ShouldReturnLogs()
    {
        var dto = new TimeRangeDto
        {
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow
        };
        var logs = new List<Devicelog> { new() { Id = "log123" } };

        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow(It.IsAny<string>()))
            .Returns(CreateJwt());

        _cleanAirServiceMock
            .Setup(s => s.GetDailyAverages(dto))
            .Returns(logs);

        var result = _controller.GetDailyAverages(dto, "auth");

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(logs));
    }

    [Test]
    public void GetDailyAverages_ServiceThrows_ShouldReturn500()
    {
        var dto = new TimeRangeDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow
        };

        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow(It.IsAny<string>()))
            .Returns(CreateJwt());

        _cleanAirServiceMock
            .Setup(s => s.GetDailyAverages(dto))
            .Throws(new Exception("fail"));

        var result = _controller.GetDailyAverages(dto, "auth");

        var error = result.Result as ObjectResult;
        
        Assert.Multiple(() =>
        {
            Assert.That(error, Is.Not.Null);
            Assert.That(error!.StatusCode, Is.EqualTo(500));
            Assert.That(error.Value, Is.EqualTo("An error occurred while retrieving daily averages."));
        });

    }
    [Test]
    public async Task AdminChangesDeviceInterval_AdminRole_ShouldReturnOk()
    { 
        var dto = new AdminChangesDeviceIntervalDto
        {
            Interval = 15
        };

        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow("admin-token"))
            .Returns(CreateJwt("admin"));

        _cleanAirServiceMock
            .Setup(s => s.UpdateDeviceIntervalAndBroadcast(dto))
            .Returns(Task.CompletedTask);

        var result = await _controller.AdminChangesDeviceInterval("admin-token", dto);

        Assert.That(result, Is.InstanceOf<OkResult>());
        _cleanAirServiceMock.Verify(s => s.UpdateDeviceIntervalAndBroadcast(dto), Times.Once);
    }

    [Test]
    public async Task AdminChangesDeviceInterval_NotAdmin_ShouldReturnUnauthorized()
    {
        var dto = new AdminChangesDeviceIntervalDto
        {
            Interval = 15
        };

        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow("user-token"))
            .Returns(CreateJwt("user"));

        var result = await _controller.AdminChangesDeviceInterval("user-token", dto);

        var unauthorized = result as UnauthorizedObjectResult;
        
        Assert.Multiple(() =>
        {
            Assert.That(unauthorized, Is.Not.Null); 
            Assert.That(unauthorized!.StatusCode, Is.EqualTo(401));
            Assert.That(unauthorized.Value, Is.EqualTo("You are not authorized to change intervals"));
        });


        _cleanAirServiceMock.Verify(s => s.UpdateDeviceIntervalAndBroadcast(It.IsAny<AdminChangesDeviceIntervalDto>()), Times.Never);
    }

    [Test]
    public async Task AdminChangesDeviceInterval_ServiceThrows_ShouldReturn500()
    {
        var dto = new AdminChangesDeviceIntervalDto
        {
            Interval = 15
        };

        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow("admin-token"))
            .Returns(CreateJwt("admin"));

        _cleanAirServiceMock
            .Setup(s => s.UpdateDeviceIntervalAndBroadcast(dto))
            .ThrowsAsync(new Exception("fail"));

        var result = await _controller.AdminChangesDeviceInterval("admin-token", dto);

        var error = result as ObjectResult;
        
        Assert.Multiple(() =>
        {
            Assert.That(error, Is.Not.Null);
            Assert.That(error!.StatusCode, Is.EqualTo(500));
            Assert.That(error.Value, Is.EqualTo("Internal server error"));
        });
    }
    
    [Test]
    public async Task DeleteData_AdminRole_ShouldReturnOk()
    {
        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow("admin-token"))
            .Returns(CreateJwt("admin"));

        _cleanAirServiceMock
            .Setup(s => s.DeleteDataAndBroadcast(It.IsAny<JwtClaims>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteData("admin-token");

        Assert.That(result, Is.InstanceOf<OkResult>());
        _cleanAirServiceMock.Verify(s => s.DeleteDataAndBroadcast(It.IsAny<JwtClaims>()), Times.Once);
    }

    [Test]
    public async Task DeleteData_NotAdmin_ShouldReturnUnauthorized()
    {
        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow("user-token"))
            .Returns(CreateJwt("user"));

        var result = await _controller.DeleteData("user-token");

        var unauthorized = result as UnauthorizedObjectResult;
        
        Assert.Multiple(() =>
        {
            Assert.That(unauthorized, Is.Not.Null);
            Assert.That(unauthorized!.StatusCode, Is.EqualTo(401));
            Assert.That(unauthorized.Value, Is.EqualTo("You are not authorized to delete data"));
        });
        
        _cleanAirServiceMock.Verify(s => s.DeleteDataAndBroadcast(It.IsAny<JwtClaims>()), Times.Never);
    }

    [Test]
    public async Task DeleteData_ServiceThrows_ShouldReturn500()
    {
        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow("admin-token"))
            .Returns(CreateJwt("admin"));

        _cleanAirServiceMock
            .Setup(s => s.DeleteDataAndBroadcast(It.IsAny<JwtClaims>()))
            .ThrowsAsync(new Exception("fail"));

        var result = await _controller.DeleteData("admin-token");

        var error = result as ObjectResult;
        
        Assert.Multiple(() =>
        {
            Assert.That(error, Is.Not.Null);
            Assert.That(error!.StatusCode, Is.EqualTo(500));
            Assert.That(error.Value, Is.EqualTo("Internal server error"));
        });

    }
   /* 
    [Test]
    public void GetLogsForToday_FeatureEnabled_ShouldReturnLogs()
    {
        // Arrange
        var timeRangeDto = new TimeRangeDto 
        { 
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow 
        };
        var authorization = "Bearer valid-token";
        var expectedLogs = new List<Devicelog> 
        { 
            new Devicelog { Id = "test", Deviceid = "device1" }
        };

        // Setup security service
        _securityServiceMock.Setup(s => s.VerifyJwtOrThrow(authorization));
    
        // Setup feature flag - enabled
        var featureMock = new Mock<IFeature>();
        featureMock.Setup(f => f.IsEnabled).Returns(true);
        _featureHubRepoMock.Setup(f => f.GetFeature("CleanFeature"))
            .Returns(featureMock.Object);

        // Setup service to return expected data
        _cleanAirServiceMock.Setup(s => s.GetLogsForToday(timeRangeDto))
            .Returns(expectedLogs);

        // Act
        var result = _controller.GetLogsForToday(timeRangeDto, authorization);

        // Assert
        var ok = result.Result as OkObjectResult;
    
        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.EqualTo(expectedLogs));
        });
    
        // Verify all mocks were called as expected
        _featureHubRepoMock.Verify(f => f.GetFeature("CleanFeature"), Times.Once);
        _securityServiceMock.Verify(s => s.VerifyJwtOrThrow(authorization), Times.Once);
        _cleanAirServiceMock.Verify(s => s.GetLogsForToday(timeRangeDto), Times.Once);
    }
    */
    [Test]
    public void GetDailyAverages_ShouldCallVerifyJwtOrThrow_AndReturnOk()
    {
        // Arrange
        var authorization = "Bearer valid-token";
        var dto = new TimeRangeDto(); // Provide real values if needed
        var expectedResult = new List<Devicelog> { new Devicelog() };

        _securityServiceMock.Setup(s => s.VerifyJwtOrThrow(authorization));
        _cleanAirServiceMock.Setup(s => s.GetDailyAverages(dto)).Returns(expectedResult);

        // Act
        var result = _controller.GetDailyAverages(dto, authorization);

        // Assert
        var ok = result.Result as OkObjectResult;
        
        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.EqualTo(expectedResult));
        });
        
        _securityServiceMock.Verify(s => s.VerifyJwtOrThrow(authorization), Times.Once);
    }
    [Test]
    public async Task GetLogs_ValidToken_ShouldReturnOkWithLogs()
    {
        // Arrange
        var authorization = "valid-token";
        var claims = CreateJwt("user");
        var logs = new List<Devicelog> { new Devicelog { Id = "log1" }, new Devicelog { Id = "log2" } };

        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow(authorization))
            .Returns(claims);
            
        _cleanAirServiceMock
            .Setup(s => s.GetDeviceFeed(claims))
            .Returns(logs);

        // Act
        var result = await _controller.GetLogs(authorization);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(okResult.Value, Is.EqualTo(logs));
        });

        _securityServiceMock.Verify(s => s.VerifyJwtOrThrow(authorization), Times.Once);
        _cleanAirServiceMock.Verify(s => s.GetDeviceFeed(claims), Times.Once);
        _loggerMock.Verify(l => l.LogInformation("[CleanAirController] GetLogs endpoint called"), Times.Once);
        _loggerMock.Verify(l => l.LogInformation($"[CleanAirController] Authorized user. Role: {claims.Role}"), Times.Once);
        _loggerMock.Verify(l => l.LogInformation($"[CleanAirController] retrieved {logs.Count} logs"), Times.Once);
    }

    [Test]
    public async Task GetLogs_InvalidToken_ShouldReturnInternalServerError()
    {
        // Arrange
        var authorization = "invalid-token";
        var exception = new Exception("Invalid token");

        _securityServiceMock
            .Setup(s => s.VerifyJwtOrThrow(authorization))
            .Throws(exception);

        // Act
        var result = await _controller.GetLogs(authorization);

        // Assert
        var errorResult = result.Result as ObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(errorResult, Is.Not.Null);
            Assert.That(errorResult.StatusCode, Is.EqualTo(500));
            Assert.That(errorResult.Value, Is.EqualTo("Internal server error"));
        });

        _securityServiceMock.Verify(s => s.VerifyJwtOrThrow(authorization), Times.Once);
        _loggerMock.Verify(l => l.LogInformation("[CleanAirController] GetLogs endpoint called"), Times.Once);
        _loggerMock.Verify(l => l.LogError("[CleanAirController] Error occurred while retrieving logs", exception), Times.Once);
    }

    
}
