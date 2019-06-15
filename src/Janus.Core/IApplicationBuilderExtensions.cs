using Janus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.EntityFrameworkCore
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder SeedDataContext<TContext>(this IApplicationBuilder app, string connectionStringConfigurationKey, Action<ContextManager> seedBuilder)
        {
            var contextManager = new ContextManager(connectionStringConfigurationKey);
            seedBuilder(contextManager);

            foreach (TestDatabaseConfiguration dbConfig in contextManager.DatabaseConfigurations)
            {
                DbContext context = (DbContext)app.ApplicationServices.GetRequiredService(dbConfig.DbContextType);
                contextManager.SeedDatabase(app.ApplicationServices, dbConfig, context);
            }

            return app;
        }
    }
}
