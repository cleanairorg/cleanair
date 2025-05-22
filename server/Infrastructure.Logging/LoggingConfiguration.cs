using Serilog;

namespace Infrastructure.Logging;

public class LoggingConfiguration
{
    public static void Configure(string? seqUrl)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Seq(seqUrl ?? "http://localhost:5341")
            .MinimumLevel.Information()
            .CreateLogger();
    }

}