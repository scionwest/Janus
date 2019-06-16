using Janus;
using Janus.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder ApplyJanusSeeding(this IApplicationBuilder app, Action<IJanusManager> seedBuilder)
        {
            IJanusManager contextManager = app.ApplicationServices.GetRequiredService<IJanusManager>();
            seedBuilder(contextManager);

            foreach (TestDatabaseConfiguration dbConfig in contextManager.DatabaseConfigurations)
            {
                using (IServiceScope scope = app.ApplicationServices.CreateScope())
                {
                    DbContext context = (DbContext)scope.ServiceProvider.GetRequiredService(dbConfig.DbContextType);
                    if (dbConfig.GenerateFreshDatabase)
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                    }

                    contextManager.SeedDatabase(app.ApplicationServices, dbConfig, context);
                }
            }

            return app;
        }
    }

    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddJanusSeeding(this IServiceCollection services, string databaseKey)
        {
            services.Configure<JanusContextOptions>(options => options.ConnectionStringDatabaseKey = databaseKey);
            services.AddSingleton<IDataContextSeeder, DataContextSeeder>();
            services.AddSingleton<IJanusManager, ContextManager>(provider =>
            {
                IOptions<JanusContextOptions> options = provider.GetService<IOptions<JanusContextOptions>>();
                JanusContextOptions contextOptions = options.Value;
                return new ContextManager(contextOptions);
            });

            return services;
        }
    }
}
