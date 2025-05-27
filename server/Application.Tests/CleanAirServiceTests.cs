using Application.Services;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.BroadcastModels;
using Application.Models;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Websocket;
using Core.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;
using Application.Interfaces.Infrastructure.Logging;
using Application.Models.Dtos.RestDtos;

namespace Application.Tests.Services
{
    [TestFixture]
    public class CleanAirServiceTests
    {
        private Mock<IOptionsMonitor<AppOptions>> _optionsMonitorMock;
        private Mock<ILoggingService> _loggerMock;
        private Mock<ICleanAirRepository> _repositoryMock;
        private Mock<IMqttPublisher> _mqttPublisherMock;
        private Mock<IConnectionManager> _connectionManagerMock;
        private CleanAirService _service;

        [SetUp]
        public void Setup()
        {
            _optionsMonitorMock = new Mock<IOptionsMonitor<AppOptions>>();
            _repositoryMock = new Mock<ICleanAirRepository>();
            _connectionManagerMock = new Mock<IConnectionManager>();
            _loggerMock = new Mock<ILoggingService>();
            _mqttPublisherMock = new Mock<IMqttPublisher>();

            _service = new CleanAirService(
                _optionsMonitorMock.Object,
                _loggerMock.Object,
                _repositoryMock.Object,
                _mqttPublisherMock.Object,
                _connectionManagerMock.Object);
        }

        [Test]
        public async Task AddToDbAndBroadcast_DtoIsNull_ShouldLogWarningAndDoNothing()
        {
            // Act
            await _service.AddToDbAndBroadcast(null);

            // Assert - Verifying that the warning log is triggered
            _loggerMock.Verify(x => x.LogWarning(
                It.Is<string>(msg => msg.Contains("AddToDbAndBroadcast, dto is null"))), Times.Once);

            // Ensure nothing was saved to the database
            _repositoryMock.Verify(r => r.AddDeviceLog(It.IsAny<Devicelog>()), Times.Never);

            // Ensure nothing was broadcasted
            _connectionManagerMock.Verify(c => c.BroadcastToTopic(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }


        [Test]
        public async Task AddToDbAndBroadcast_ValidDto_ShouldAddToRepoAndBroadcast()
        {
            // Arrange
            var dto = new CollectDataDto
            {
                DeviceId = "dev123",
                Temperature = 25.1f,
                Humidity = 50.5f,
                Pressure = 1013.3f,
                AirQuality = 1
            };

            var recentLog = new Devicelog { Id = "log1" };

            _repositoryMock.Setup(r => r.GetLatestLogs()).Returns(recentLog);

            // Act
            await _service.AddToDbAndBroadcast(dto);

            // Assert - Verify data is added to db
            _repositoryMock.Verify(r => r.AddDeviceLog(It.Is<Devicelog>(log =>
                log.Deviceid == dto.DeviceId &&
                log.Temperature == (decimal)dto.Temperature &&
                log.Humidity == (decimal)dto.Humidity &&
                log.Pressure == (decimal)dto.Pressure &&
                log.Airquality == (int)dto.AirQuality
            )), Times.Once);

            // Verify broadcast of latest measurement
            _connectionManagerMock.Verify(c => c.BroadcastToTopic(
                StringConstants.Dashboard,
                It.Is<ServerBroadcastsLatestReqestedMeasurement>(b => b.LatestMeasurement == recentLog)
            ), Times.Once);

            // Verify logging
            _loggerMock.Verify(x => x.LogInformation(
                It.Is<string>(msg => msg.Contains("Added DeviceLog to Database"))), Times.Once);
            _repositoryMock.Verify(r => r.AddDeviceLog(It.Is<Devicelog>(log =>
                log.Unit == "Celsius"
            )), Times.Once);

        }

        [Test]
        public void GetLogsForToday_ValidRange_ShouldReturnLogs()
        {
            // Arrange
            var dto = new TimeRangeDto
            {
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow
            };

            var expectedLogs = new List<Devicelog>
            {
                new Devicelog { Id = "log1" },
                new Devicelog { Id = "log2" }
            };

            _repositoryMock.Setup(r => r.GetLogsForToday(dto)).Returns(expectedLogs);

            // Act
            var result = _service.GetLogsForToday(dto);

            // Assert
            Assert.That(result, Is.EqualTo(expectedLogs));
            _repositoryMock.Verify(r => r.GetLogsForToday(dto), Times.Once);
            _loggerMock.Verify(x => x.LogInformation(
                It.Is<string>(msg => msg.Contains("returned"))), Times.Once);
        }

        [Test]
        public void GetLogsForToday_InvalidRange_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = new TimeRangeDto
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _service.GetLogsForToday(dto));
            Assert.That(ex.Message, Is.EqualTo("StartDate cannot be after EndDate."));
        }


        [Test]
        public void GetDailyAverages_InvalidRange_ShouldThrowArgumentException()
        {
            var now = DateTime.UtcNow;
            var invalid = new TimeRangeDto { StartDate = now.AddDays(1), EndDate = now };
            var equal = new TimeRangeDto { StartDate = now, EndDate = now };

            Assert.Throws<ArgumentException>(() => _service.GetDailyAverages(invalid));
            Assert.Throws<ArgumentException>(() => _service.GetDailyAverages(equal));
        }


        [Test]
        public void GetLogsForToday_NoLogs_ShouldReturnEmptyList()
        {
            // Arrange
            var dto = new TimeRangeDto
            {
                StartDate = DateTime.UtcNow.AddHours(-12),
                EndDate = DateTime.UtcNow
            };

            _repositoryMock.Setup(r => r.GetLogsForToday(dto)).Returns(new List<Devicelog>());

            // Act
            var result = _service.GetLogsForToday(dto);

            // Assert
            Assert.That(result, Is.Empty);
            _repositoryMock.Verify(r => r.GetLogsForToday(dto), Times.Once);
        }

        [Test]
        public void GetLogsForToday_RepositoryThrows_ShouldLogAndRethrow()
        {
            // Arrange
            var dto = new TimeRangeDto
            {
                StartDate = DateTime.UtcNow.AddHours(-1),
                EndDate = DateTime.UtcNow
            };

            _repositoryMock
                .Setup(r => r.GetLogsForToday(dto))
                .Throws(new InvalidOperationException("Repo error"));

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _service.GetLogsForToday(dto));
            Assert.That(ex.Message, Is.EqualTo("Repo error"));

            // Verify the logger was called with both arguments
            _loggerMock.Verify(x => x.LogError(
                It.Is<string>(msg => msg.Contains("Error in GetLogsForToday")),
                It.IsAny<Exception?>()), Times.Once);
        }


        [Test]
        public void GetDailyAverages_NoData_ShouldReturnEmptyList()
        {
            // Arrange
            var dto = new TimeRangeDto
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };

            _repositoryMock.Setup(r => r.GetDailyAverages(dto)).Returns(new List<Devicelog>());

            // Act
            var result = _service.GetDailyAverages(dto);

            // Assert
            Assert.That(result, Is.Empty);
            _repositoryMock.Verify(r => r.GetDailyAverages(dto), Times.Once);
        }


        [Test]
        public void GetDailyAverages_ValidRange_ShouldLogDto()
        {
            // Arrange
            var dto = new TimeRangeDto
            {
                StartDate = DateTime.UtcNow.AddDays(-3),
                EndDate = DateTime.UtcNow
            };

            var resultList = new List<Devicelog> { new Devicelog { Id = "test" } };

            _repositoryMock.Setup(r => r.GetDailyAverages(dto)).Returns(resultList);

            // Act
            var result = _service.GetDailyAverages(dto);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(
                It.Is<string>(msg => msg.Contains("GetDailyAverages with DTO"))), Times.Once);


            Assert.That(result, Is.EqualTo(resultList));
        }

        [Test]
        public void GetDailyAverages_StartDateEqualsEndDate_ShouldThrowArgumentException()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new TimeRangeDto
            {
                StartDate = now,
                EndDate = now
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _service.GetDailyAverages(dto));
            Assert.That(ex.Message, Is.EqualTo("StartDate cannot be after EndDate."));
        }

        [Test]
        public void GetDailyAverages_ShouldLogReturnedRecordCount()
        {
            // Arrange
            var dto = new TimeRangeDto
            {
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow
            };

            var fakeLogs = new List<Devicelog>
            {
                new Devicelog { Id = "d1" },
                new Devicelog { Id = "d2" }
            };

            _repositoryMock.Setup(r => r.GetDailyAverages(dto)).Returns(fakeLogs);

            string? capturedLog = null;
            _loggerMock.Setup(x => x.LogInformation(It.IsAny<string>()))
                .Callback<string>(msg => capturedLog = msg);

            // Act
            var result = _service.GetDailyAverages(dto);

            // Assert
            Assert.That(result, Is.EqualTo(fakeLogs));
            Assert.IsNotNull(capturedLog);
            StringAssert.Contains("returned", capturedLog);
            StringAssert.Contains("2", capturedLog);
        }

        [Test]
        public void GetLogsForToday_StartDateEqualsEndDate_ShouldNotThrow()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new TimeRangeDto
            {
                StartDate = now,
                EndDate = now
            };

            _repositoryMock.Setup(r => r.GetLogsForToday(dto)).Returns(new List<Devicelog>());

            // Act & Assert
            Assert.DoesNotThrow(() => _service.GetLogsForToday(dto));
        }

        [Test]
        public void GetLogsForToday_ShouldLogInputDto()
        {
            // Arrange
            var dto = new TimeRangeDto
            {
                StartDate = DateTime.UtcNow.AddHours(-2),
                EndDate = DateTime.UtcNow
            };

            _repositoryMock.Setup(r => r.GetLogsForToday(dto)).Returns(new List<Devicelog>());

            var logs = new List<string>();
            _loggerMock.Setup(x => x.LogInformation(It.IsAny<string>()))
                .Callback<string>(msg => logs.Add(msg));

            // Act
            _service.GetLogsForToday(dto);

            // Assert
            Assert.That(logs, Is.Not.Empty, "Expected at least one log message");
            var inputLog = logs.FirstOrDefault(l => l.Contains("called with DTO"));
            Assert.IsNotNull(inputLog, "Expected a log containing 'called with DTO'");
            StringAssert.Contains("StartDate", inputLog); // confirms serialized DTO content
        }
        

        [Test]
        public async Task GetMeasurementNowAndBroadcast_ShouldPublishAndBroadcastLatestMeasurement()
        {
            // Arrange
            var latestLog = new Devicelog { Id = "log123" };
            _repositoryMock.Setup(r => r.GetLatestLogs()).Returns(latestLog);

            object? publishedDto = null;
            string? publishedTopic = null;

            _mqttPublisherMock
                .Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<string>()))
                .Callback<object, string>((dto, topic) =>
                {
                    publishedDto = dto;
                    publishedTopic = topic;
                })
                .Returns(Task.CompletedTask);

            ServerBroadcastsLatestReqestedMeasurement? broadcastSent = null;

            _connectionManagerMock
                .Setup(c => c.BroadcastToTopic(
                    It.Is<string>(s => s == StringConstants.Dashboard),
                    It.IsAny<ServerBroadcastsLatestReqestedMeasurement>()))
                .Callback<string, object>((_, msg) =>
                {
                    broadcastSent = msg as ServerBroadcastsLatestReqestedMeasurement;
                })
                .Returns(Task.CompletedTask);

            // Act
            await _service.GetMeasurementNowAndBroadcast();

            // Assert: MQTT publish call
            Assert.That(publishedTopic, Is.EqualTo("cleanair/measurement/now"));
            Assert.That(publishedDto, Is.EqualTo("1"));
            _mqttPublisherMock.Verify(p => p.Publish("1", "cleanair/measurement/now"), Times.Once);

            // Assert: WebSocket broadcast
            Assert.IsNotNull(broadcastSent, "Expected broadcast to be sent");
            Assert.That(broadcastSent!.LatestMeasurement, Is.EqualTo(latestLog));
            _connectionManagerMock.Verify(c => c.BroadcastToTopic(StringConstants.Dashboard,
                It.Is<ServerBroadcastsLatestReqestedMeasurement>(b => b.LatestMeasurement == latestLog)), Times.Once);
        }

        [Test]
        public async Task DeleteDataAndBroadcast_ShouldDeleteDataAndBroadcast()
        {
            // Arrange
            var jwt = new JwtClaims
            {
                Id = "admin123",
                Role = "admin",
                Email = "admin@example.com",
                Exp = ((DateTimeOffset)DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds().ToString()
            };

            // Act
            await _service.DeleteDataAndBroadcast(jwt);

            // Assert
            _repositoryMock.Verify(r => r.DeleteAllData(), Times.Once);

            _connectionManagerMock.Verify(cm =>
                cm.BroadcastToTopic(
                    StringConstants.Dashboard,
                    It.Is<AdminHasDeletedData>(data => data != null)), Times.Once);
        }

		[Test]
		public void GetLatestDeviceLog_LogExists_ShouldReturnLogAndLogInfo()
		{
   			// Arrange
   			var log = new Devicelog { Id = "log123" };
   			_repositoryMock.Setup(r => r.GetLatestLogs()).Returns(log);

  		 	// Act
   		 	var result = _service.GetLatestDeviceLog();

  		 	// Assert
   		 	Assert.That(result, Is.EqualTo(log));

   			_loggerMock.Verify(l => l.LogInformation(
        	It.Is<string>(msg => msg.Contains("Attempting to get the latest device log"))), Times.Once);

    		_loggerMock.Verify(l => l.LogInformation(
        	It.Is<string>(msg => msg.Contains($"got the latest device log found, LogID: {log.Id}"))), Times.Once);
		}

		[Test]
		public void GetLatestDeviceLog_NoLogExists_ShouldReturnNullAndLogWarning()
		{
    		// Arrange
    		_repositoryMock.Setup(r => r.GetLatestLogs()).Returns((Devicelog)null!);

    		// Act
    		var result = _service.GetLatestDeviceLog();

    		// Assert
    		Assert.IsNull(result);

   			_loggerMock.Verify(l => l.LogInformation(
       		It.Is<string>(msg => msg.Contains("Attempting to get the latest device log"))), Times.Once);

    		_loggerMock.Verify(l => l.LogWarning(
        	It.Is<string>(msg => msg.Contains("No device logs found"))), Times.Once);
		}

	 	[Test]
		public void GetLatestDeviceLog_RepositoryThrows_ShouldLogErrorAndRethrow()
		{
    		// Arrange
    		var exception = new Exception("Database failure");
    		_repositoryMock.Setup(r => r.GetLatestLogs()).Throws(exception);

    		// Act & Assert
    		var ex = Assert.Throws<Exception>(() => _service.GetLatestDeviceLog());
    		Assert.That(ex!.Message, Is.EqualTo("Database failure"));

    		_loggerMock.Verify(l => l.LogError(
        	It.Is<string>(msg => msg.Contains("Failed to get the latest log")),
        	exception), Times.Once);
		}
		
		[Test]
		public async Task UpdateDeviceIntervalAndBroadcast_ValidDto_ShouldPublishAndBroadcast()
		{
   			// Arrange
    		var dto = new AdminChangesDeviceIntervalDto { Interval = 30 };

    		// Act
    		await _service.UpdateDeviceIntervalAndBroadcast(dto);

    		// Assert: MQTT publish
    		_mqttPublisherMock.Verify(p => p.Publish(dto.Interval, StringConstants.ChangeInterval), Times.Once);

    		// Assert: WebSocket broadcast
    		_connectionManagerMock.Verify(c =>
       		c.BroadcastToTopic(StringConstants.Dashboard,
       	    It.Is<ServerBroadcastsIntervalChange>(b => b.Interval == dto.Interval)), Times.Once);

    		// Assert: Logs
    		_loggerMock.Verify(l => l.LogInformation(
        	It.Is<string>(msg => msg.Contains("Updating device interval"))), Times.Once);

    		_loggerMock.Verify(l => l.LogInformation(
        	It.Is<string>(msg => msg.Contains("broadcasting interval changes to dashboard"))), Times.Once);

    		_loggerMock.Verify(l => l.LogInformation(
        	It.Is<string>(msg => msg.Contains("Updated interval"))), Times.Once);
		}

		[Test]
		public void UpdateDeviceIntervalAndBroadcast_PublishThrows_ShouldLogErrorAndRethrow()
		{
    		// Arrange
    		var dto = new AdminChangesDeviceIntervalDto { Interval = 15 };
    		var ex = new Exception("Publish failed");

    		_mqttPublisherMock.Setup(p => p.Publish(dto.Interval, StringConstants.ChangeInterval))
        	.Throws(ex);

    		// Act & Assert
    		var thrown = Assert.ThrowsAsync<Exception>(async () =>
       		await _service.UpdateDeviceIntervalAndBroadcast(dto));

    		Assert.That(thrown!.Message, Is.EqualTo("Publish failed"));

    		// Verify error log
    		_loggerMock.Verify(l => l.LogError(
        	It.Is<string>(msg => msg.Contains("An error occurred while updating device interval")),
        	ex), Times.Once);
		}


	}
    
}
