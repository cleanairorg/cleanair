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
    
    [Test]
    public void GetDailyAverages_OrderShouldNotAffectGrouping()
    {
        var today = DateTime.UtcNow.Date;
        _context.Devicelogs.AddRange(
            new Devicelog { Deviceid = "dev1", Unit = "Celsius", Id = "1", Timestamp = today.AddHours(3), Temperature = 24, Humidity = 60, Pressure = 1012, Airquality = 3 },
            new Devicelog { Deviceid = "dev1", Unit = "Celsius", Id = "2", Timestamp = today.AddHours(1), Temperature = 20, Humidity = 40, Pressure = 1008, Airquality = 2 }
        );
        _context.SaveChanges();

        var range = new TimeRangeDto { StartDate = today, EndDate = today.AddDays(1) };
        var result = _repository.GetDailyAverages(range);

        result.Should().HaveCount(1);
        result[0].Temperature.Should().BeApproximately(22, 0.1m);
    }
    
    [Test]
    public void GetDailyAverages_ExactlyAtStartTime_ShouldIncludeData()
    {
        var from = DateTime.UtcNow.Date;
        _context.Devicelogs.Add(new Devicelog
        {
            Id = "start-included",
            Deviceid = "dev123",
            Unit = "Celsius",
            Timestamp = from,
            Temperature = 21,
            Humidity = 40,
            Pressure = 1005,
            Airquality = 1
        });
        _context.SaveChanges();

        var dto = new TimeRangeDto { StartDate = from, EndDate = from.AddHours(1) };
        var result = _repository.GetDailyAverages(dto);

        result.Should().NotBeEmpty();
    }
    
    [Test]
    public void GetDailyAverages_DataOutsideRange_ShouldBeExcluded()
    {
        var from = DateTime.UtcNow.Date;
        var before = from.AddDays(-1);
        var after = from.AddDays(2);

        _context.Devicelogs.AddRange(
            new Devicelog { Id = "before", Deviceid = "dev1", Unit = "Celsius", Timestamp = before, Temperature = 10 },
            new Devicelog { Id = "after", Deviceid = "dev1", Unit = "Celsius", Timestamp = after, Temperature = 30 }
        );
        _context.SaveChanges();

        var dto = new TimeRangeDto { StartDate = from, EndDate = from.AddDays(1) };
        var result = _repository.GetDailyAverages(dto);

        result.Should().BeEmpty();
    }
    
    [Test]
    public void GetDailyAverages_ExactlyAtEndDate_ShouldBeIncluded()
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(1).AddTicks(-1); // one tick before midnight

        _context.Devicelogs.Add(new Devicelog
        {
            Id = "end-edge",
            Deviceid = "dev1",
            Unit = "Celsius",
            Timestamp = end,
            Temperature = 25
        });
        _context.SaveChanges();

        var dto = new TimeRangeDto { StartDate = start, EndDate = end };
        var result = _repository.GetDailyAverages(dto);

        result.Should().NotBeEmpty();
    }

    [Test]
    public void GetDailyAverages_ShouldUseAverageNotMin()
    {
        var today = DateTime.UtcNow.Date;
        _context.Devicelogs.AddRange(
            new Devicelog { Id = "1", Deviceid = "dev1", Unit = "Celsius", Timestamp = today.AddHours(1), Humidity = 40 },
            new Devicelog { Id = "2", Deviceid = "dev1", Unit = "Celsius", Timestamp = today.AddHours(2), Humidity = 60 }
        );
        _context.SaveChanges();

        var dto = new TimeRangeDto { StartDate = today, EndDate = today.AddDays(1) };
        var result = _repository.GetDailyAverages(dto);

        result.Should().ContainSingle()
            .Which.Humidity.Should().BeApproximately(50, 0.1m);
    }

    [Test]
    public void GetDailyAverages_UnitShouldBeCelsius()
    {
        var today = DateTime.UtcNow.Date;
        _context.Devicelogs.Add(new Devicelog
        {
            Id = "1",
            Deviceid = "dev1",
            Unit = "Celsius",
            Timestamp = today,
            Temperature = 20,
            Humidity = 50,
            Pressure = 1010,
            Airquality = 2
        });
        _context.SaveChanges();

        var dto = new TimeRangeDto { StartDate = today, EndDate = today.AddDays(1) };
        var result = _repository.GetDailyAverages(dto);

        result.Should().ContainSingle().Which.Unit.Should().Be("Celsius");
    }

    [Test]
    public void GetDailyAverages_OrderShouldNotAffectAverages()
    {
        var today = DateTime.UtcNow.Date;
        _context.Devicelogs.AddRange(
            new Devicelog { Id = "1", Deviceid = "dev1", Unit = "Celsius", Timestamp = today.AddHours(2), Temperature = 20, Humidity = 40, Pressure = 1010, Airquality = 1 },
            new Devicelog { Id = "2", Deviceid = "dev1", Unit = "Celsius", Timestamp = today.AddHours(1), Temperature = 30, Humidity = 60, Pressure = 1020, Airquality = 3 }
        );
        _context.SaveChanges();

        var range = new TimeRangeDto { StartDate = today, EndDate = today.AddDays(1) };

        var result = _repository.GetDailyAverages(range);

        result.Should().HaveCount(1);

        // Average of 20 and 30 = 25
        result[0].Temperature.Should().BeInRange(24.9m, 25.1m);
    }

    
    [Test]
    public void GetDailyAverages_PressureShouldUseAverage_NotMin()
    {
        var today = DateTime.UtcNow.Date;
        _context.Devicelogs.AddRange(
            new Devicelog { Id = "1", Deviceid = "dev1", Unit = "Celsius", Timestamp = today.AddHours(1), Pressure = 1000 },
            new Devicelog { Id = "2", Deviceid = "dev1", Unit = "Celsius", Timestamp = today.AddHours(2), Pressure = 1020 }
        );
        _context.SaveChanges();

        var range = new TimeRangeDto { StartDate = today, EndDate = today.AddDays(1) };

        var result = _repository.GetDailyAverages(range);

        result.Should().ContainSingle()
            .Which.Pressure.Should().BeApproximately(1010, 0.1m);
    }


    [Test]
    public void GetLogsForToday_OutOfRangeTimestamps_ShouldNotBeIncluded()
    {
        var now = DateTime.UtcNow;
        var log = new Devicelog { Id = "1", Deviceid = "dev", Unit = "C", Timestamp = now.AddHours(-2) };
        _context.Devicelogs.Add(log);
        _context.SaveChanges();

        var dto = new TimeRangeDto { StartDate = now.AddHours(-1), EndDate = now };

        var result = _repository.GetLogsForToday(dto);

        result.Should().BeEmpty(); 
    }

    [Test]
    public void GetLogsForToday_BoundariesAndOrder_ShouldBeCorrect()
    {
        var now = DateTime.UtcNow;
        var logs = new[]
        {
            new Devicelog { Id = "1", Deviceid = "dev", Unit = "C", Timestamp = now.AddHours(-1) },
            new Devicelog { Id = "2", Deviceid = "dev", Unit = "C", Timestamp = now }
        };
        _context.Devicelogs.AddRange(logs);
        _context.SaveChanges();

        var dto = new TimeRangeDto { StartDate = now.AddHours(-1), EndDate = now };
        var result = _repository.GetLogsForToday(dto);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be("1");
        result[1].Id.Should().Be("2");
    }


    

    
}
