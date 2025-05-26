using Application.Models;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface ICleanAirService
{
    List<Devicelog> GetDeviceFeed(JwtClaims client);
    Task AddToDbAndBroadcast(CollectDataDto? dto);
    Devicelog GetLatestDeviceLog();
    Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims);
    Task UpdateDeviceIntervalAndBroadcast(AdminChangesDeviceIntervalDto dto);
    Task DeleteDataAndBroadcast(JwtClaims jwt);
    Task GetMeasurementNowAndBroadcast();
    List<Devicelog> GetDailyAverages(TimeRangeDto dto);
    List<Devicelog> GetLogsForToday(TimeRangeDto timeRangeDto);
}