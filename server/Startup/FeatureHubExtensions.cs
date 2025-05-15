using Microsoft.Extensions.Options;
using Application.Models;
using FeatureHubSDK;

public static class FeatureHubExtensions
{
    public static IServiceCollection RegisterFeatureHub(this IServiceCollection services)
    {
        services.AddSingleton<IFeatureHubRepository>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<AppOptions>>().Value;
            var fhConfig = new EdgeFeatureHubConfig(config.FEATUREHUB_URL, config.FEATUREHUB_API_KEY);
            return fhConfig.Repository;
        });

        return services;
    }
}