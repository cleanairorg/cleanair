using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class Extensions
{
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IWeatherStationService, WeatherStationService>();
        services.AddScoped<IWebsocketSubscriptionService, WebsocketSubscriptionService>();
        services.AddScoped<IThresholdService, ThresholdService>();
        services.AddScoped<IThresholdEvaluator, ThresholdEvaluator>();
        return services;
    }
}