using Infrastructure.LoggingExtensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Infrastructure.Filters
{
    public class TrackActionPerformanceFilter : IActionFilter
    {
        private Stopwatch timer;
        private readonly ILogger<TrackActionPerformanceFilter> logger;

        public TrackActionPerformanceFilter(ILogger<TrackActionPerformanceFilter> logger)
        {
            this.logger = logger;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            timer = new Stopwatch();
            timer.Start();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            timer.Stop();
            if (context.Exception == null)
            {
                logger.LogRoutePerformance(context.HttpContext.Request.Path,
                    context.HttpContext.Request.Method,
                    timer.ElapsedMilliseconds);
            }
        }
    }
}
