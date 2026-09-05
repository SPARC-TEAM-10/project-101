using Microsoft.Extensions.Logging;
using Moq;

namespace Chh.Infrastructure.Tests.Common;

/// <summary>Captures every formatted message logged to a Moq'd <see cref="ILogger{T}"/>, so tests can assert on log content (e.g. "the OTP code never appears in a log line").</summary>
public static class MockLoggerCapture
{
    /// <summary>Wires up capture and returns the list every formatted log message will be appended to.</summary>
    /// <param name="logger">The mocked logger to capture from.</param>
    public static List<string> CaptureMessages<T>(this Mock<ILogger<T>> logger)
    {
        var messages = new List<string>();
        logger.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(new InvocationAction(invocation =>
            {
                var state = invocation.Arguments[2];
                var exception = (Exception?)invocation.Arguments[3];
                var formatter = invocation.Arguments[4];
                var invokeMethod = formatter.GetType().GetMethod("Invoke");
                if (invokeMethod?.Invoke(formatter, new[] { state, exception }) is string message)
                {
                    messages.Add(message);
                }
            }));
        return messages;
    }
}
