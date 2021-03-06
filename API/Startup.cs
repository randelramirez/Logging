using Core;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Persistence;
using Persistence.Services;
using Serilog;
using System.Reflection;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers() // We need to NewtonsoftJson for Patch processing
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }); 

            services.AddScoped<IContactService, ContactService>();
            var contextAssembly = typeof(DataContext).GetTypeInfo().Assembly.GetName().Name;
            services.AddDbContextPool<DataContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString(nameof(DataContext)), b => b.MigrationsAssembly(contextAssembly));
                //options.UseInMemoryDatabase(databaseName: nameof(DataContext));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.SeedDataContext();
            }

            // This allows to create a response for unhandled exceptions
            // Note: this also overrides UseDeveloperExceptionPage
            app.UseApiExceptionHandler(options =>
            {
                //options.AddResponseDetails = UpdateApiErrorResponse;

                options.AddResponseDetails = (context, ex, error) =>
                {
                    if (ex.GetType().Name == nameof(SqlException))
                    {
                        error.Detail = "Exception was a database exception!";
                    }
                    //error.Links = "https://gethelpformyerror.com/";
                };
            });

            //static void UpdateApiErrorResponse(HttpContext context, Exception ex, ApiError error)
            //{
            //    if (ex.GetType().Name == nameof(SqlException))
            //    {
            //        error.Detail = "Exception was a database exception!";
            //    }
            //    //error.Links = "https://gethelpformyerror.com/";
            //}

            // middleware logging
            app.UseSerilogRequestLogging(options =>
            {
                // Customize the message template
                //options.MessageTemplate = "Handled {RequestPath}";

                // Emit debug-level events instead of the defaults
                //options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

                // Attach additional properties to the request completion event
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                };
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
