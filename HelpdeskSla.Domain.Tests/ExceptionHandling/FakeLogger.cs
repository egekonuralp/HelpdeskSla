using Microsoft.Extensions.Logging;

namespace HelpdeskSla.Domain.Tests.ExceptionHandling;

public sealed class FakeLogger<T> : ILogger<T>
{
    public List<LogLevel> LoggedLevels { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LoggedLevels.Add(logLevel);
    }
}