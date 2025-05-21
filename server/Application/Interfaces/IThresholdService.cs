using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IThresholdService
{
    Task UpdateThresholdAndBroadcastAsync(AdminUpdatesThresholdsDto adminUpdatesThresholdsDto);
}