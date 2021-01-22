using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using System.Linq;

namespace API
{
    public static class StartupHelper
    {
        public static void SeedDataContext(this IApplicationBuilder application)
        {
            using (var serviceScope = application.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<DataContext>();

                context.Database.EnsureCreated();

                var databaseSeeder = new DataSeeder(context);
                if (!context.Contacts.Any())
                {
                    databaseSeeder.SeedContacts();
                }
            }
        }
    }
}
