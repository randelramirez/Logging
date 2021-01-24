using Microsoft.Extensions.Logging;

namespace Infrastructure.LoggerExtensions
{
    public class DataEvents
    {
        public static EventId GetAllUsingRawSqlQuery = new EventId(10001, "GetAllUsingRawSqlQuery");
    }
}
