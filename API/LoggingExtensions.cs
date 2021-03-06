﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;
using Serilog.Sinks.MSSqlServer;
using System.IO;
using System.Reflection;

namespace API
{
    public static class LoggingExtensions
    {
        public static IHostBuilder UseSerilogForLogging(this IHostBuilder hostBuilder)
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            hostBuilder.UseSerilog((hostingContext, services, loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
                      .Enrich.FromLogContext()
                      .Enrich.WithMachineName() // usefule for distributed/microservice 
                      .Enrich.WithProperty(nameof(Assembly), assembly.Name)
                      .Enrich.WithProperty("Version", assembly.Version)
                      .WriteTo.MSSqlServer(hostingContext.Configuration.GetConnectionString("DataContext"), 
                        sinkOptions: GetMSSqlServerSinkOptions(), 
                        columnOptions: GetColumnOptions())
                      .WriteTo.Logger(l => l.WriteTo.MSSqlServer(hostingContext.Configuration.GetConnectionString("DataContext"),
                        sinkOptions: GetMSSqlServerSinkOptions2(),
                        columnOptions: GetColumnOptions())
                      .Filter.ByIncludingOnly(Matching.WithProperty("ErrorId"))) // see ApiExceptionMiddleware for {ErrorId}
                      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // minimize the logs that we see(we only show logs from MS namespace if warning and above)
                      .WriteTo.File(new JsonFormatter(), Path.Combine(Directory.GetCurrentDirectory(), "logs.json"), shared: true,
                          restrictedToMinimumLevel: LogEventLevel.Warning);  // this means any log level below Warning will not be displayed on the file (Warning level and above are shown)

                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        // We can only see the logs in the console if we're running in kestrel
                        //loggerConfiguration.WriteTo.Console(new JsonFormatter(),Serilog.Events.LogEventLevel.Verbose);

                        // this means any log level below Information will not be displayed on the Console (information level and above are shown)
                        loggerConfiguration.WriteTo.Console(Serilog.Events.LogEventLevel.Information);

                        // this means any log level below Information will not be displayed on the Debug (information level and above are shown)
                        loggerConfiguration.WriteTo.Debug(Serilog.Events.LogEventLevel.Information);
                    }
                });

            return hostBuilder;
        }

        private static ColumnOptions GetColumnOptions()
        {
            var options = new ColumnOptions();
            // some customization
            options.Level.ColumnName = "LogLevel";
            options.Level.DataLength = -1;

            options.Store.Remove(StandardColumn.Properties); // We do not want the xml
            options.Store.Add(StandardColumn.LogEvent); // this is the json log message that we see when we're logging in file
            return options;
        }

        private static MSSqlServerSinkOptions GetMSSqlServerSinkOptions()
        {
            var options = new MSSqlServerSinkOptions();
            options.AutoCreateSqlTable = true;
            options.TableName = "Logs";
            return options;
        }

        private static MSSqlServerSinkOptions GetMSSqlServerSinkOptions2()
        {
            // ONLY LOGS with ErrorId property will be logged
            var options = new MSSqlServerSinkOptions();
            options.AutoCreateSqlTable = true;
            options.TableName = "LogsWithFilter";
            return options;
        }
    }
}
