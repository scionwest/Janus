using Janus;
using Janus.Seeding;
using Microsoft.AspNetCore.Builder;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IApplicationBuilderExtensions
    {
        public static IDatabaseManager ApplyJanusDatabase(this IApplicationBuilder app)
        {
            IDatabaseManager contextManager = app.ApplicationServices.GetRequiredService<IDatabaseManager>();
            return contextManager;

            //foreach (TestDatabaseConfiguration dbConfig in contextManager.DatabaseConfigurations)
            //{
            //    using (IServiceScope scope = app.ApplicationServices.CreateScope())
            //    {
            //        DbContext context = (DbContext)scope.ServiceProvider.GetRequiredService(dbConfig.DbContextType);
            //        if (dbConfig.GenerateFreshDatabase)
            //        {
            //            context.Database.EnsureDeleted();
            //            context.Database.EnsureCreated();
            //        }

            //        contextManager.SeedDatabase(app.ApplicationServices, dbConfig, context);
            //    }
            //}
            //return app;
        }
    }

    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddJanusDatabaseSeeding(this IServiceCollection services)
        {
            services.AddJanusDatabaseSeeding(options =>
            {
                options.ConnectionStringOptions.ConfigurationKey = "Default";
                options.DatabaseBehavior = DatabaseBehavior.AlwaysRetain;
            });

            return services;
        }

        public static IServiceCollection AddJanusDatabaseSeeding(this IServiceCollection services, Action<DatabaseBuilderOptions> options)
        {
            services.AddSingleton<IDatabaseManager, JanusDatabaseManager>();
            services.AddSingleton<IDatabaseBuilderFactory, JanusDatabaseBuilderFactory>();
            services.AddSingleton<IDatabaseSeedWriter>();
            services.AddSingleton<IDatabaseSeedReader>();

            services.Configure(options);

            return services;
        }
    }
}
