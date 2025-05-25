using Application.Interfaces;
using Application.Services;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.BroadcastModels;
using Application.Models;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Websocket;
using Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Models.Dtos.RestDtos;

namespace Application.Tests.Services
{
    [TestFixture]
    public class CleanAirServiceTests
    {
		private Mock<IOptionsMonitor<AppOptions>> _optionsMonitorMock;
		private Mock<ILogger<CleanAirService>> _loggerMock;
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
            _loggerMock = new Mock<ILogger<CleanAirService>>();
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

            // Assert - Verifying that the logWarning is being triggered/logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dto is null")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            //verifying nothing is being saved in the database
            _repositoryMock.Verify(r => r.AddDeviceLog(It.IsAny<Devicelog>()), Times.Never);
            //verifying that nothing is broadcasted
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
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Added DeviceLog to Database")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
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
            // Arrange
            var dto = new TimeRangeDto
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _service.GetDailyAverages(dto));
            Assert.That(ex.Message, Is.EqualTo("StartDate cannot be after EndDate."));
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

            _repositoryMock.Setup(r => r.GetLogsForToday(dto)).Throws(new InvalidOperationException("Repo error"));

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _service.GetLogsForToday(dto));
            Assert.That(ex.Message, Is.EqualTo("Repo error"));

            // Ensure logger captured the error
            _loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in GetLogsForToday")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
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
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetDailyAverages with DTO")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

            Assert.That(result, Is.EqualTo(resultList));
        }


    }
 }