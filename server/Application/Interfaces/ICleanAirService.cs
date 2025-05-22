using Application.Models;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface ICleanAirService
{
    List<Devicelog> GetDeviceFeed(JwtClaims client);
    Devicelog GetLatestDeviceLog();
    Task AddToDbAndBroadcast(CollectDataDto? dto);
    Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims);
    Task DeleteDataAndBroadcast(JwtClaims jwt);
    Task GetMeasurementNowAndBroadcast();
}