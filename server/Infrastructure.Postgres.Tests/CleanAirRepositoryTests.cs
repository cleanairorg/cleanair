using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using FluentAssertions;
using Infrastructure.Postgres.Postgresql.Data;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Infrastructure.Postgres.Tests;

[TestFixture]
public class CleanAirRepositoryTests
{
    private MyDbContext _context;
    private CleanAirRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new MyDbContext(options);
        _repository = new CleanAirRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public void AddDeviceLog_ShouldAddLogToDatabase()
    {
        var log = new Devicelog
        {
            Deviceid = "dev123",
            Id = "log1",
            Timestamp = DateTime.UtcNow,
            Temperature = 22.5m,
            Humidity = 50.0m,
            Pressure = 1013.0m,
            Airquality = 2,
            Unit = "Celsius"
        };

        var result = _repository.AddDeviceLog(log);

        result.Should().Be(log);

        var savedLog = _context.Devicelogs.Find("log1");
        savedLog.Should().NotBeNull();
        savedLog.Temperature.Should().Be(22.5m);
    }

    [Test]
    public void GetLatestLogs_WithLogs_ShouldReturnMostRecent()
    {
        var older = new Devicelog {Deviceid = "dev123", Unit = "Celcius", Id = "1", Timestamp = DateTime.UtcNow.AddMinutes(-10) };
        var newer = new Devicelog {Deviceid = "dev123", Unit = "Celcius", Id = "2", Timestamp = DateTime.UtcNow };
        _context.Devicelogs.AddRange(older, newer);
        _context.SaveChanges();

        var result = _repository.GetLatestLogs();

        result.Should().NotBeNull();
        result.Id.Should().Be("2");
    }

    [Test]
    public void GetLogsForToday_WithinRange_ShouldReturnLogs()
    {
        var now = DateTime.UtcNow;
        var logs = new[]
        {
            new Devicelog { Deviceid = "dev123", Unit = "Celcius", Id = "1", Timestamp = now.AddHours(-1) },
            new Devicelog { Deviceid = "dev123", Unit = "Celcius", Id = "2", Timestamp = now }
        };

        _context.Devicelogs.AddRange(logs);
        _context.SaveChanges();

        var range = new TimeRangeDto
        {
            StartDate = now.AddHours(-2),
            EndDate = now.AddMinutes(10)
        };

        var result = _repository.GetLogsForToday(range);

        result.Should().HaveCount(2);
    }

    [Test]
    public void GetDailyAverages_WithValidData_ShouldReturnAverages()
    {
        var today = DateTime.UtcNow.Date;
        _context.Devicelogs.AddRange(
            new Devicelog { Deviceid = "dev123", Unit = "Celcius", Id = "1", Timestamp = today.AddHours(1), Temperature = 20, Humidity = 50, Pressure = 1010, Airquality = 2 },
            new Devicelog { Deviceid = "dev123", Unit = "Celcius", Id = "2", Timestamp = today.AddHours(2), Temperature = 22, Humidity = 55, Pressure = 1012, Airquality = 3 });
        _context.SaveChanges();

        var range = new TimeRangeDto { StartDate = today, EndDate = today.AddDays(1) };

        var result = _repository.GetDailyAverages(range);

        result.Should().HaveCount(1);
        result[0].Temperature.Should().BeApproximately(21, 0.1m);
    }

    [Test]
    public async Task DeleteAllData_ShouldClearDatabase()
    {
        _context.Devicelogs.Add(new Devicelog { Deviceid = "dev123", Unit = "Celcius", Id = "deleteMe", Timestamp = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        await _repository.DeleteAllData();

        _context.Devicelogs.Should().BeEmpty();
    }
}
