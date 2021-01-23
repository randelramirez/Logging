using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Middleware
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ApiExceptionMiddleware> logger;
        private readonly ApiExceptionOptions options;

        public ApiExceptionMiddleware(ApiExceptionOptions options, RequestDelegate next,
            ILogger<ApiExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
            this.options = options;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, options);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception, ApiExceptionOptions opts)
        {
            var error = new ApiError
            {
                Id = Guid.NewGuid().ToString(),
                Status = (short)HttpStatusCode.InternalServerError,
                Title = "Some kind of error occurred in the API.  Please use the id and contact our " +
                        "support team if the problem persists."
            };

            // we setup AddResponseDetails on Startup.cs
            opts.AddResponseDetails?.Invoke(context, exception, error);

            var innerExMessage = GetInnermostExceptionMessage(exception);

            logger.LogError(exception, "ApiExceptionMiddleware!!! " + innerExMessage + " -- {ErrorId}.", error.Id);

            var result = JsonConvert.SerializeObject(error);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(result);
        }

        private string GetInnermostExceptionMessage(Exception exception)
        {
            if (exception.InnerException != null)
                return GetInnermostExceptionMessage(exception.InnerException);

            return exception.Message;
        }
    }
}
