using Janus;
using Janus.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder ApplyDbContextSeeding(this IApplicationBuilder app, string connectionStringConfigurationKey, Action<ContextManager> seedBuilder)
        {
            var contextManager = new ContextManager(connectionStringConfigurationKey);
            seedBuilder(contextManager);

            foreach (TestDatabaseConfiguration dbConfig in contextManager.DatabaseConfigurations)
            {
                using (IServiceScope scope = app.ApplicationServices.CreateScope())
                {
                    DbContext context = (DbContext)scope.ServiceProvider.GetRequiredService(dbConfig.DbContextType);
                    contextManager.SeedDatabase(app.ApplicationServices, dbConfig, context);
                }
            }

            return app;
        }
    }

    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContextSeeding(this IServiceCollection services)
        {
            services.AddSingleton<IDataContextSeeder, DataContextSeeder>();
            return services;
        }
    }
}
