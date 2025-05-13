using Application.Interfaces.Infrastructure.Logging;
using Serilog;

namespace Infrastructure.Logging;

public class LoggingService : ILoggingService
{
    private readonly Serilog.ILogger _logger;

    public LoggingService()
    {
        _logger = Log.Logger;
    }

    public void LogInformation(string message)
    {
        _logger.Information(message);
    }

    public void LogWarning(string message)
    {
        _logger.Warning(message);
    }


    public void LogError(string message, Exception? exception = null)
    {
        if (exception != null)
            _logger.Error(exception, message);
        else
            _logger.Error("{Message} (no exception attached)", message);
    }

    public void LogDebug(string message)
    {
        _logger.Debug(message);
    }
}