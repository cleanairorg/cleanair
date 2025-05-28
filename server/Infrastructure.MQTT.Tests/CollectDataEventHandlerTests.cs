using NUnit.Framework;
using Moq;
using Infrastructure.MQTT.SubscriptionEventHandlers;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.Types;
using HiveMQtt.MQTT5.Packets;
using System;
using System.Text.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Interfaces;
    

namespace Infrastructure.MQTT.Tests;

[TestFixture]
public class CollectDataEventHandlerTests
{
    private Mock<ICleanAirService> _cleanAirServiceMock = null!;
    private CollectDataEventHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _cleanAirServiceMock = new Mock<ICleanAirService>();
        _handler = new CollectDataEventHandler(_cleanAirServiceMock.Object);
    }

    private static OnMessageReceivedEventArgs CreateEventArgs(string payload)
    {
        var publishMessage = new MQTT5PublishMessage("CollectData", QualityOfService.AtLeastOnceDelivery)
        {
            Payload = Encoding.UTF8.GetBytes(payload)
        };

        return new OnMessageReceivedEventArgs(publishMessage);
    }


    [Test]
    public void Handle_ValidPayload_ShouldCallAddToDbAndBroadcast()
    {
        var dto = new
        {
            DeviceId = "device1",
            Temperature = 22.5f,
            Humidity = 45.0f,
            Pressure = 1012.3f,
            AirQuality = 88.0f,
            Interval = 5
        };

        string jsonPayload = JsonSerializer.Serialize(dto);
        var eventArgs = CreateEventArgs(jsonPayload);

        _handler.Handle(this, eventArgs);

        _cleanAirServiceMock.Verify(s => s.AddToDbAndBroadcast(It.Is<CollectDataDto>(d =>
            d.DeviceId == dto.DeviceId &&
            d.Temperature == dto.Temperature &&
            d.Humidity == dto.Humidity &&
            d.Pressure == dto.Pressure &&
            d.AirQuality == dto.AirQuality &&
            d.Interval == dto.Interval
        )), Times.Once);
    }

    [Test]
    public void Handle_InvalidJson_ShouldThrowException()
    {
        var invalidJson = "{ this is not valid json }";
        var eventArgs = CreateEventArgs(invalidJson);

        Assert.Throws<JsonException>(() => _handler.Handle(this, eventArgs));
    }

    [Test]
    public void Handle_MissingRequiredField_ShouldThrowValidationException()
    {
        var dto = new
        {
            DeviceId = "",
            Temperature = 20.0f,
            Humidity = 50.0f,
            Pressure = 1000.0f,
            AirQuality = 80.0f,
            Interval = 10
        };

        string jsonPayload = JsonSerializer.Serialize(dto);
        var eventArgs = CreateEventArgs(jsonPayload);

        Assert.Throws<ValidationException>(() => _handler.Handle(this, eventArgs));
    }
}
