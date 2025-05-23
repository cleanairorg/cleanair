using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IThresholdService
{
    Task UpdateThresholdsAndBroadcastAsync(AdminUpdatesThresholdsDto adminUpdatesThresholdsDto);
    Task<ThresholdsBroadcastDto> GetThresholdsWithEvaluationAsync(string deviceId);
}