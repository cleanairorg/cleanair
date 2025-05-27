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

            // Start streaming connection (we don't use the result directly here)
            fhConfig.NewContext().Build().GetAwaiter().GetResult();

            return fhConfig.Repository;
        });

        return services;
    }
}