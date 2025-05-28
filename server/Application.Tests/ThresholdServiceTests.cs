using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.SharedDtos;
using Application.Models.Enums;
using Application.Services;
using Core.Domain.Entities;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Application.Tests;

[TestFixture]
public class ThresholdServiceTests
{
    private Mock<IDeviceThresholdRepository> _thresholdRepo = null!;
    private Mock<ICleanAirRepository> _logRepo = null!;
    private Mock<IMqttPublisher> _mqtt = null!;
    private Mock<IThresholdEvaluator> _evaluator = null!;
    private Mock<ILoggingService> _logger = null!;
    private ThresholdService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _thresholdRepo = new Mock<IDeviceThresholdRepository>();
        _logRepo = new Mock<ICleanAirRepository>();
        _mqtt = new Mock<IMqttPublisher>();
        _evaluator = new Mock<IThresholdEvaluator>();
        _logger = new Mock<ILoggingService>();

        _service = new ThresholdService(
            _thresholdRepo.Object,
            _logRepo.Object,
            _mqtt.Object,
            _evaluator.Object,
            _logger.Object);
    }

    [Test]
    public async Task UpdateThresholdsAsync_ValidInput_UpdatesThresholdsAndPublishesMqtt()
    {
        // Arrange
        var dto = new AdminUpdatesThresholdsDto
        {
            Thresholds = new List<ThresholdDto>
            {
                new()
                {
                    Metric = "airquality",
                    GoodMin = 400,
                    GoodMax = 1000,
                    WarnMin = 1000,
                    WarnMax = 2000
                }
            }
        };
        _logRepo.Setup(r => r.GetCurrentLogAsync()).ReturnsAsync(new Devicelog());

        // Act
        await _service.UpdateThresholdsAsync(dto);

        // Assert
        _thresholdRepo.Verify(r => r.UpdateThresholdAsync(It.Is<DeviceThreshold>(
            t => t.Metric == "airquality" && t.GoodMax == 1000 && t.WarnMax == 2000)), Times.Once);

        _mqtt.Verify(m => m.Publish(
            It.Is<AdminUpdatesDeviceThresholdsDto>(p =>
                p.GoodMax == 1000 && p.WarnMax == 2000),
            It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void UpdateThresholdsAsync_NoCurrentLog_ThrowsException()
    {
        // Arrange
        var dto = new AdminUpdatesThresholdsDto
        {
            Thresholds = new List<ThresholdDto>
            {
                new() { Metric = "airquality", GoodMax = 1000, WarnMax = 2000 }
            }
        };
        _logRepo.Setup(r => r.GetCurrentLogAsync()).ReturnsAsync((Devicelog?)null);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateThresholdsAsync(dto));
    }
    
    [TestCase("temperature", 10, 18, 25, ThresholdStates.CriticalLow)]
    [TestCase("temperature", 17, 18, 25, ThresholdStates.WarningLow)]
    [TestCase("temperature", 18, 18, 25, ThresholdStates.Good)]
    [TestCase("temperature", 22, 18, 25, ThresholdStates.Good)]
    [TestCase("temperature", 26, 18, 25, ThresholdStates.WarningHigh)]
    [TestCase("temperature", 35, 18, 25, ThresholdStates.CriticalHigh)]
    public async Task GetThresholdsWithEvaluationsAsync_EvaluatesMetricCorrectly(
        string metric, decimal value, decimal goodMin, decimal goodMax, ThresholdStates expected)
    {
        // Arrange
        var threshold = new DeviceThreshold { Metric = metric, GoodMin = goodMin, GoodMax = goodMax };
        var log = new Devicelog
        {
            Temperature = metric == "temperature" ? value : 0,
            Humidity = metric == "humidity" ? value : 0,
            Pressure = metric == "pressure" ? value : 0,
            Airquality = metric == "airquality" ? (int)value : 0
        };
        var eval = new ThresholdEvaluationResult { Metric = metric, Value = value, State = expected };

        _thresholdRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DeviceThreshold> { threshold });
        _logRepo.Setup(r => r.GetCurrentLogAsync()).ReturnsAsync(log);
        _evaluator.Setup(e => e.Evaluate(metric, value, threshold)).Returns(eval);

        // Act
        var result = await _service.GetThresholdsWithEvaluationsAsync();

        // Assert
        result.Evaluations.Single().Should().BeEquivalentTo(eval);
    }
    
    [TestCase("humidity", 55, 40, 60, ThresholdStates.Good)]
    [TestCase("pressure", 990, 1000, 1020, ThresholdStates.WarningLow)]
    [TestCase("airquality", 2600, 0, 1200, ThresholdStates.CriticalHigh)]
    public async Task GetThresholdsWithEvaluationsAsync_EvaluatesNonTemperatureMetrics(
        string metric, decimal value, decimal goodMin, decimal goodMax, ThresholdStates expected)
    {
        // Arrange
        var threshold = new DeviceThreshold { Metric = metric, GoodMin = goodMin, GoodMax = goodMax };

        var log = new Devicelog
        {
            Temperature = 0,
            Humidity = metric == "humidity" ? value : 0,
            Pressure = metric == "pressure" ? value : 0,
            Airquality = metric == "airquality" ? (int)value : 0
        };

        var eval = new ThresholdEvaluationResult { Metric = metric, Value = value, State = expected };

        _thresholdRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DeviceThreshold> { threshold });
        _logRepo.Setup(r => r.GetCurrentLogAsync()).ReturnsAsync(log);
        _evaluator.Setup(e => e.Evaluate(metric, value, threshold)).Returns(eval);

        // Act
        var result = await _service.GetThresholdsWithEvaluationsAsync();

        // Assert
        result.Evaluations.Single().Should().BeEquivalentTo(eval);
    }

    
    [Test]
    public async Task UpdateThresholdsAsync_WithNullThresholdList_DoesNotUpdateThresholds()
    {
        // Arrange
        _logRepo.Setup(r => r.GetCurrentLogAsync()).ReturnsAsync(new Devicelog());
        var dto = new AdminUpdatesThresholdsDto { Thresholds = null };

        // Act
        await _service.UpdateThresholdsAsync(dto);

        // Assert
        _thresholdRepo.Verify(r => r.UpdateThresholdAsync(It.IsAny<DeviceThreshold>()), Times.Never);
    }

    
    [Test]
    public async Task UpdateThresholdsAsync_WithNullThresholdList_UsesFallbackThresholdsInMqtt()
    {
        // Arrange
        _logRepo.Setup(r => r.GetCurrentLogAsync()).ReturnsAsync(new Devicelog());
        var dto = new AdminUpdatesThresholdsDto { Thresholds = null };

        // Act
        await _service.UpdateThresholdsAsync(dto);

        // Assert
        _mqtt.Verify(m => m.Publish(
            It.Is<AdminUpdatesDeviceThresholdsDto>(d =>
                d.GoodMax == 1200 && d.WarnMax == 2500),
            It.IsAny<string>()), Times.Once);
    }



}
