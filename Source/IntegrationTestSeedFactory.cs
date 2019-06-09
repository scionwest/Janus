using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspNetCore.IntegrationTestSeeding
{
    public class IntegrationTestSeedFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string userSecretId;
        private List<TestDatabaseConfiguration> contexts = new List<TestDatabaseConfiguration>();

        public IntegrationTestSeedFactory(string userSecretId) => this.userSecretId = userSecretId;
        public IntegrationTestSeedFactory() { }

        public IntegrationTestSeedFactory<TStartup> RetainDatabase<TContext>() where TContext : DbContext
        {
            TestDatabaseConfiguration dbConfig = this.contexts.FirstOrDefault(config => config.DbContextType == typeof(TContext));
            if (dbConfig == null) return this;

            dbConfig.RetainDatabase = true;
            return this;
        }

        public IntegrationTestSeedFactory<TStartup> ForDataContext<TContext>(string configurationConnectionStringKey, [CallerMemberName] string executingTest = "")
        {
            var dbConfig = new TestDatabaseConfiguration
            {
                ConfigurationConnectionStringKey = configurationConnectionStringKey,
                DbContextType = typeof(TContext),
                ExecutingTest = executingTest,
            };

            this.contexts.Add(dbConfig);
            return this;
        }

        public IntegrationTestSeedFactory<TStartup> WithSeedData<TContext>(Action<TContext> seeder) where TContext : DbContext
        {
            TestDatabaseConfiguration dbConfig = this.contexts.FirstOrDefault(config => config.DbContextType == typeof(TContext));
            if (dbConfig == null) return this;

            dbConfig.DatabaseSeeder = seeder;
            return this;
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            foreach(TestDatabaseConfiguration dbConfig in this.contexts)
            {
                this.RenameConnectionStringDatabase(builder, dbConfig.ConfigurationConnectionStringKey);
            }

            TestServer server = base.CreateServer(builder);

            foreach (TestDatabaseConfiguration dbConfig in this.contexts)
            {
                DbContext context = (DbContext)server.Host.Services.GetService(dbConfig.DbContextType);
                context.Database.EnsureCreated();
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
