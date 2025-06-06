using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Interfaces;
using Application.Models;
using Application.Models.Dtos.MqttSubscriptionDto;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.Types;

namespace Infrastructure.MQTT.SubscriptionEventHandlers;

public class CollectDataEventHandler(ICleanAirService cleanAirService) : IMqttMessageHandler
{
    public string TopicFilter { get; } = StringConstants.CollectData;
    public QualityOfService QoS { get; } = QualityOfService.AtLeastOnceDelivery;
    public void Handle(object? sender, OnMessageReceivedEventArgs args)
    {
        var dto = JsonSerializer.Deserialize<CollectDataDto>(args.PublishMessage.PayloadAsString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Could not deserialize into " + nameof(CollectDataDto) + " from " +
                                      args.PublishMessage.PayloadAsString);
        var context = new ValidationContext(dto);
        Validator.ValidateObject(dto, context);
        cleanAirService.AddToDbAndBroadcast(dto);
        
        // Print data for testing purposes
        Console.WriteLine("Parsed data:");
        Console.WriteLine($"Device ID: {dto.DeviceId}");
        Console.WriteLine($"Temperature: {dto.Temperature}°C");
        Console.WriteLine($"Humidity: {dto.Humidity}%");
        Console.WriteLine($"Pressure: {dto.Pressure} hPa");
        Console.WriteLine($"Air Quality: {dto.AirQuality}");
        Console.WriteLine("-------------------------");
        
    }
}