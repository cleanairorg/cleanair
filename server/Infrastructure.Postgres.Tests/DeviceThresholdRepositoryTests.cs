using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Postgres.Scaffolding;
using Infrastructure.Postgres.Postgresql.Data;
using Core.Domain.Entities;
using FluentAssertions;

namespace Infrastructure.Postgres.Tests;

[TestFixture]
public class DeviceThresholdRepositoryTests
{
    private MyDbContext _context;
    private DeviceThresholdRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new MyDbContext(options);
        _repository = new DeviceThresholdRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetAllAsync_WithNoThresholds_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllAsync_WithThresholds_ReturnsAllThresholds()
    {
        // Arrange
        var thresholds = new List<DeviceThreshold>
        {
            new DeviceThreshold 
            { 
                Id = Guid.NewGuid().ToString(),
                Metric = "Temperature",
                WarnMin = 5,
                GoodMin = 10,
                GoodMax = 25,
                WarnMax = 30
            },
            new DeviceThreshold 
            { 
                Id = Guid.NewGuid().ToString(),
                Metric = "Humidity",
                WarnMin = 20,
                GoodMin = 30,
                GoodMax = 70,
                WarnMax = 80
            }
        };

        _context.DeviceThresholds.AddRange(thresholds);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Metric == "Temperature");
        result.Should().Contain(t => t.Metric == "Humidity");
    }

    [Test]
    public async Task UpdateThresholdAsync_WithExistingThreshold_UpdatesValues()
    {
        // Arrange
        var existingThreshold = new DeviceThreshold
        {
            Id = Guid.NewGuid().ToString(),
            Metric = "Temperature",
            WarnMin = 5,
            GoodMin = 10,
            GoodMax = 25,
            WarnMax = 30
        };

        _context.DeviceThresholds.Add(existingThreshold);
        await _context.SaveChangesAsync();

        var updatedThreshold = new DeviceThreshold
        {
            Metric = "Temperature",
            WarnMin = 0,
            GoodMin = 15,
            GoodMax = 28,
            WarnMax = 35
        };

        // Act
        await _repository.UpdateThresholdAsync(updatedThreshold);

        // Assert
        var result = await _context.DeviceThresholds
            .FirstAsync(t => t.Metric == "Temperature");

        result.WarnMin.Should().Be(0);
        result.GoodMin.Should().Be(15);
        result.GoodMax.Should().Be(28);
        result.WarnMax.Should().Be(35);
        result.Id.Should().Be(existingThreshold.Id); // ID should remain the same
    }

    [Test]
    public async Task UpdateThresholdAsync_WithNonExistingThreshold_CreatesNewThreshold()
    {
        // Arrange
        var newThreshold = new DeviceThreshold
        {
            Metric = "Pressure",
            WarnMin = 900,
            GoodMin = 1000,
            GoodMax = 1020,
            WarnMax = 1100
        };

        // Act
        await _repository.UpdateThresholdAsync(newThreshold);

        // Assert
        var result = await _context.DeviceThresholds
            .FirstOrDefaultAsync(t => t.Metric == "Pressure");

        result.Should().NotBeNull();
        result.Metric.Should().Be("Pressure");
        result.WarnMin.Should().Be(900);
        result.GoodMin.Should().Be(1000);
        result.GoodMax.Should().Be(1020);
        result.WarnMax.Should().Be(1100);
        result.Id.Should().NotBe(Guid.Empty.ToString()); // Should have generated new ID
    }

    [Test]
    public async Task UpdateThresholdAsync_WithValidData_DoesNotThrow()
    {
        // Arrange
        var threshold = new DeviceThreshold
        {
            Metric = "Temperature",
            WarnMin = 5,
            GoodMin = 10,
            GoodMax = 25,
            WarnMax = 30
        };

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => 
            await _repository.UpdateThresholdAsync(threshold));
        
        // Verify it was saved
        var result = await _repository.GetAllAsync();
        result.Should().HaveCount(1);
        result.First().Metric.Should().Be("Temperature");
    }

    [Test]
    public async Task UpdateThresholdAsync_UpdatesMultipleThresholds_SavesCorrectly()
    {
        // Arrange
        var threshold1 = new DeviceThreshold
        {
            Metric = "Temperature",
            WarnMin = 5,
            GoodMin = 10,
            GoodMax = 25,
            WarnMax = 30
        };

        var threshold2 = new DeviceThreshold
        {
            Metric = "Humidity",
            WarnMin = 20,
            GoodMin = 30,
            GoodMax = 70,
            WarnMax = 80
        };

        // Act
        await _repository.UpdateThresholdAsync(threshold1);
        await _repository.UpdateThresholdAsync(threshold2);

        // Assert
        var allThresholds = await _repository.GetAllAsync();
        allThresholds.Should().HaveCount(2);

        var tempThreshold = allThresholds.First(t => t.Metric == "Temperature");
        var humidityThreshold = allThresholds.First(t => t.Metric == "Humidity");

        tempThreshold.WarnMax.Should().Be(30);
        humidityThreshold.WarnMax.Should().Be(80);
    }

    [Test]
    public async Task UpdateThresholdAsync_WithSameMetricTwice_UpdatesSameRecord()
    {
        // Arrange
        var initialThreshold = new DeviceThreshold
        {
            Metric = "Temperature",
            WarnMin = 5,
            GoodMin = 10,
            GoodMax = 25,
            WarnMax = 30
        };

        await _repository.UpdateThresholdAsync(initialThreshold);

        var updatedThreshold = new DeviceThreshold
        {
            Metric = "Temperature", // Same metric
            WarnMin = 0,
            GoodMin = 5,
            GoodMax = 30,
            WarnMax = 35
        };

        // Act
        await _repository.UpdateThresholdAsync(updatedThreshold);

        // Assert
        var allThresholds = await _repository.GetAllAsync();
        allThresholds.Should().HaveCount(1); // Should still be only one record

        var result = allThresholds.First();
        result.Metric.Should().Be("Temperature");
        result.WarnMin.Should().Be(0); // Should have updated values
        result.WarnMax.Should().Be(35);
    }
}