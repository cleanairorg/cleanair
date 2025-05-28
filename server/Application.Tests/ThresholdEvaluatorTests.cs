using Application.Models.Dtos.SharedDtos;
using Application.Models.Enums;
using Application.Services;
using Core.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Application.Tests;

[TestFixture]
public class ThresholdEvaluatorTests
{
    private ThresholdEvaluator _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new ThresholdEvaluator();
    }

    [TestCase(2, 5, 10, 12, ThresholdStates.CriticalLow)]
    [TestCase(4, 5, 10, 12, ThresholdStates.WarningLow)]
    [TestCase(5, 5, 10, 12, ThresholdStates.Good)]
    [TestCase(10, 5, 10, 12, ThresholdStates.Good)]
    [TestCase(11, 5, 10, 12, ThresholdStates.WarningHigh)]
    [TestCase(13, 5, 10, 12, ThresholdStates.CriticalHigh)]
    public void Evaluate_ReturnsCorrectState(
        decimal value, decimal goodMin, decimal goodMax, decimal warnMax, ThresholdStates expected)
    {
        // Arrange
        var threshold = new DeviceThreshold
        {
            WarnMin = 3,
            GoodMin = goodMin,
            GoodMax = goodMax,
            WarnMax = warnMax
        };

        // Act
        var result = _service.Evaluate("temperature", value, threshold);

        // Assert
        result.Metric.Should().Be("temperature");
        result.Value.Should().Be(value);
        result.State.Should().Be(expected);
    }
}