using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                    .WriteTo.File(new JsonFormatter(), Path.Combine(Directory.GetCurrentDirectory(), "logs.json"), shared: true);

                  if (hostingContext.HostingEnvironment.IsDevelopment())
                  {
                      // We can only see the logs in the console if we're running in kestrel
                      loggerConfiguration.WriteTo.Console(Serilog.Events.LogEventLevel.Verbose);
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
