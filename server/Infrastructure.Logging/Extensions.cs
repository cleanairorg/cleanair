using Application.Interfaces.Infrastructure.Logging;

namespace Infrastructure.Logging;

public static class Extensions
{
    public static IServiceCollection RegisterLoggingService (this IServiceCollection services)
    {
        services.AddSingleton<ILoggingService, LoggingService>();
        return services;
    }
}