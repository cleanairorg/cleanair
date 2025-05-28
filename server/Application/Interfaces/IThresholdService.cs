using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IThresholdService
{
    Task UpdateThresholdsAsync(AdminUpdatesThresholdsDto adminUpdatesThresholdsDto);
    Task<ThresholdsBroadcastDto> GetThresholdsWithEvaluationsAsync();
}