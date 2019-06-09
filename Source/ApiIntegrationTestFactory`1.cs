using Janus.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Janus
{

    public class ApiIntegrationTestFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string userSecretId;
        private List<TestDatabaseConfiguration> contexts = new List<TestDatabaseConfiguration>();

        public ApiIntegrationTestFactory<TStartup> RetainDatabase<TContext>() where TContext : DbContext
        {
            TestDatabaseConfiguration dbConfig = this.contexts.FirstOrDefault(config => config.DbContextType == typeof(TContext));
            if (dbConfig == null) return this;

            dbConfig.RetainDatabase = true;
            return this;
        }

        public DbContextSeedBuilder<TContext> ForDataContext<TContext>(string configurationConnectionStringKey, [CallerMemberName] string executingTest = "") where TContext : DbContext
        {
            var dbConfig = new TestDatabaseConfiguration
            {
                ConfigurationConnectionStringKey = configurationConnectionStringKey,
                DbContextType = typeof(TContext),
                ExecutingTest = executingTest,
            };

            var seedBuilder = new DbContextSeedBuilder<TContext>(dbConfig);
            dbConfig.SeedBuilder = seedBuilder;
            this.contexts.Add(dbConfig);
            return seedBuilder;
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            foreach (TestDatabaseConfiguration dbConfig in this.contexts)
            {
                this.RenameConnectionStringDatabase(builder, dbConfig.ConfigurationConnectionStringKey);
            }

            TestServer server = base.CreateServer(builder);

            foreach (TestDatabaseConfiguration dbConfig in this.contexts)
            {
                DbContext context = (DbContext)server.Host.Services.GetService(dbConfig.DbContextType);
                context.Database.EnsureCreated();

                // Resolve a context seeder and seed the database with the individual seeders
                IDataContextSeeder contextSeeder = server.Host.Services.GetRequiredService<IDataContextSeeder>();
                contextSeeder.SeedDataContext(context);

                // Always run the callback seeder last so that individual tests have the ability to replace data
                // inserted via seeders.
                dbConfig.DatabaseSeeder?.DynamicInvoke(context);
            }
            return server;

        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            IWebHostBuilder builder = base.CreateWebHostBuilder();
            builder.ConfigureAppConfiguration(configBuilder =>
            {
                if (string.IsNullOrEmpty(this.userSecretId))
                {
                    return;
                }

                configBuilder.AddUserSecrets(this.userSecretId);
            });

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<DataContextSeeder>();
            });
            return builder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot(".");
            base.ConfigureWebHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            foreach (TestDatabaseConfiguration dbConfig in this.contexts)
            {
                if (dbConfig.RetainDatabase)
                {
                    continue;
                }

                DbContext context = (DbContext)this.Server.Host.Services.GetService(dbConfig.DbContextType);
                context.Database.EnsureDeleted();

            }
            base.Dispose(disposing);
        }

        protected virtual void RenameConnectionStringDatabase(IWebHostBuilder hostBuilder, string configurationConnectionStringKey)
        {
            hostBuilder.ConfigureAppConfiguration((hostContext, configBuilder) =>
            {
                IConfiguration configuration = configBuilder.Build();
                string connectionString = configuration.GetConnectionString(configurationConnectionStringKey);

                string timeStamp = DateTime.Now.ToString("HHmmss.fff");
                string dbName = this.GetDatabaseNameFromConnectionString(connectionString);
                string newDbName = $"Tests-{dbName}-{timeStamp}";
                connectionString = this.ReplaceDatabaseNameOnConnectionString(newDbName, connectionString);

                var connectionStringConfig = new Dictionary<string, string>()
                {
                    { configurationConnectionStringKey, connectionString }
                };

                configBuilder.AddInMemoryCollection(connectionStringConfig);
            });
        }

        protected virtual string GetDatabaseNameFromConnectionString(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            return connectionStringBuilder.InitialCatalog;
        }

        protected virtual string ReplaceDatabaseNameOnConnectionString(string newDbName, string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            connectionStringBuilder.InitialCatalog = newDbName;
            return connectionStringBuilder.ToString();
        }
    }
}
