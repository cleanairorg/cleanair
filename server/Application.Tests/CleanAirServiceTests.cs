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

            var recentLogs = new List<Devicelog> { new Devicelog { Id = "log1" } };
            _repositoryMock.Setup(r => r.GetRecentLogs()).Returns(recentLogs);

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
            //verify that it is being broadcasted
            _connectionManagerMock.Verify(c => c.BroadcastToTopic(
                StringConstants.Dashboard,
                It.Is<ServerBroadcastsLiveDataToDashboard>(b => b.Logs == recentLogs)), Times.Once);
            //verify that the logger service is working
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Added DeviceLog to Database")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}