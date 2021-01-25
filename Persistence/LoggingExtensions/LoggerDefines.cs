using Core;
using Infrastructure.LoggerExtensions;
using Microsoft.Extensions.Logging;
using System;

namespace Persistence.LoggerExtensions
{
    public static class LoggerDefines
    {
        private static readonly Action<ILogger, Exception> logGetAllContacts; // this is not reusable, messages are tightly coupled with IContactService
        private static readonly Action<ILogger, string, Exception> logGetAllUsingSqlQuery;
        private static readonly Func<ILogger, string, IDisposable> _apiGetAllBooksScope;

        static LoggerDefines()
        {
            logGetAllContacts = LoggerMessage.Define(LogLevel.Information, 0,
                $"Inside the service about to call {nameof(IContactService.GetAllAsync)}.");

            logGetAllUsingSqlQuery = LoggerMessage.Define<string>(LogLevel.Information, DataEvents.GetAllUsingRawSqlQuery,
                "Debugging information for query: {queryName}");

            _apiGetAllBooksScope = LoggerMessage.DefineScope<string>(
                "Constructing books response for {ScopedUserId}");
        }

        public static void LogGetAllContacts(this ILogger logger, Exception exception = null)
        {
            if(exception != null)
            {
                logGetAllContacts(logger, exception);
            }
            else
            {
                logGetAllContacts(logger, null);
            }
        }

        public static void LogGetAllUsingRawSql(this ILogger logger, string sqlQuery, Exception exception = null)
        {
            if (exception != null)
            {
                logGetAllUsingSqlQuery(logger, sqlQuery, exception);
            }
            else
            {
                logGetAllUsingSqlQuery(logger, sqlQuery, null);
            }
        }

        public static IDisposable ApiGetAllBooksScope(this ILogger logger, string userId)
        {
            return _apiGetAllBooksScope(logger, userId);
        }
    }
}
