using System.Text.Json;
using Api.Rest;
using Api.Websocket;
using Application;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Models;
using Infrastructure.Logging;
using Infrastructure.MQTT;
using Infrastructure.Postgres;
using Infrastructure.Websocket;
using Microsoft.Extensions.Options;
using NSwag.Generation;
using Scalar.AspNetCore;
using Serilog;
using Startup.Documentation;
using Startup.Proxy;

namespace Startup;

public class Program
{
    public static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        
        var appOptions = builder.Configuration.GetSection("AppOptions").Get<AppOptions>()!;
        LoggingConfiguration.Configure(appOptions.SeqUrl); // Sets configuration

        builder.Host.UseSerilog();  // sets standard Asp net logger to use serilog
        
        ConfigureServices(builder.Services, builder.Configuration); 
        var app = builder.Build();
        await ConfigureMiddleware(app);
        
        var logger = app.Services.GetRequiredService<ILoggingService>(); 
        logger.LogInformation("Logger Service\ud83d\udd25 is running.\ud83d\ude80");
        Log.Information("SEQ URL used at startup: {SeqUrl}", appOptions.SeqUrl);
        
        await app.RunAsync();
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var appOptions = services.AddAppOptions(configuration);


        services.RegisterFeatureHub();

        services.RegisterApplicationServices();
        services.RegisterLoggingService();
        services.AddDataSourceAndRepositories();
        services.AddWebsocketInfrastructure();
        
        services.RegisterWebsocketApiServices();
        services.RegisterRestApiServices();
        if (!string.IsNullOrEmpty(appOptions.MQTT_BROKER_HOST))
        {
            services.RegisterMqttInfrastructure();
        }
        else
        {
            Console.WriteLine("Skipping MQTT service registration: Making sure there is an available mock publisher");
            services.AddSingleton<IMqttPublisher, MockMqttPublisher>();
        }

        services.AddOpenApiDocument(conf =>
        {
            conf.DocumentProcessors.Add(new AddAllDerivedTypesProcessor());
            conf.DocumentProcessors.Add(new AddStringConstantsProcessor());
        });
        services.AddSingleton<IProxyConfig, ProxyConfig>();
    }

    public static async Task ConfigureMiddleware(WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<IOptionsMonitor<AppOptions>>>();
        var appOptions = app.Services.GetRequiredService<IOptionsMonitor<AppOptions>>().CurrentValue;
        logger.LogInformation(JsonSerializer.Serialize(appOptions));
        using (var scope = app.Services.CreateScope())
        {
            if (appOptions.Seed)
                await scope.ServiceProvider.GetRequiredService<Seeder>().Seed();
        }


        app.Urls.Clear();
        app.Urls.Add($"http://0.0.0.0:{appOptions.REST_PORT}");
        app.Services.GetRequiredService<IProxyConfig>()
            .StartProxyServer(appOptions.PORT, appOptions.REST_PORT, appOptions.WS_PORT);

        app.ConfigureRestApi();
        if (!string.IsNullOrEmpty(appOptions.MQTT_BROKER_HOST))
            await app.ConfigureMqtt();
        else
            Console.WriteLine("Skipping MQTT service configuration");
        await app.ConfigureWebsocketApi(appOptions.WS_PORT);


        app.MapGet("Acceptance", () => "Accepted");

        app.UseOpenApi(conf => { conf.Path = "openapi/v1.json"; });

        var document = await app.Services.GetRequiredService<IOpenApiDocumentGenerator>().GenerateAsync("v1");
        var json = document.ToJson();
        await File.WriteAllTextAsync("openapi.json", json);
        app.GenerateTypeScriptClient("/../../client/src/generated-client.ts").GetAwaiter().GetResult();
        app.MapScalarApiReference();
    }
}