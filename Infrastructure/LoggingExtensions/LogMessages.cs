using Microsoft.Extensions.Logging;
using System;

namespace Infrastructure.LoggingExtensions
{
    public static class LogMessages
    {
        private static readonly Action<ILogger, string, string, long, Exception> routePerformance;
        static LogMessages()
        {
            routePerformance = LoggerMessage.Define<string, string, long>(LogLevel.Information, 0,
                "{RouteName} {Method} code took {ElapsedMilliseconds} ms.");
        }

        public static void LogRoutePerformance(this ILogger logger, string pageName, string method,
            long elapsedMilliseconds)
        {
            routePerformance(logger, pageName, method, elapsedMilliseconds, null);
        }
    }
}
