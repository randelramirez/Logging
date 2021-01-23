using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.IO;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
              .UseSerilog((hostingContext, services, loggerConfiguration) =>
              {
                  loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
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
              })
              .Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
