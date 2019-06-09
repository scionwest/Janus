using Janus.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Janus
{

    public class ApiIntegrationTestFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private const string DefaultDatabaseKey = "Initial Catalog";

        private readonly string connectionStringDatabaseKey;
        private List<TestDatabaseConfiguration> contexts = new List<TestDatabaseConfiguration>();

        public ApiIntegrationTestFactory() : this(DefaultDatabaseKey)
        { }

        public ApiIntegrationTestFactory(string connectionStringDatabaseKey)
        {
            this.connectionStringDatabaseKey = string.IsNullOrEmpty(connectionStringDatabaseKey)
                ? DefaultDatabaseKey
                : connectionStringDatabaseKey;
        }

        public ApiIntegrationTestFactory<TStartup> RetainDatabase<TContext>() where TContext : DbContext
        {
            TestDatabaseConfiguration dbConfig = this.contexts.FirstOrDefault(config => config.DbContextType == typeof(TContext));
            if (dbConfig == null) return this;

            dbConfig.RetainDatabase = true;
            return this;
        }

        public DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, [CallerMemberName] string executingTest = "") where TContext : DbContext
            => this.WithDataContext<TContext>(configurationConnectionStringKey, this.connectionStringDatabaseKey, executingTest);

        public DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, string connectionStringDatabaseKey, [CallerMemberName] string executingTest = "") where TContext : DbContext
        {
            var dbConfig = new TestDatabaseConfiguration
            {
                ConfigurationConnectionStringKey = configurationConnectionStringKey,
                DbContextType = typeof(TContext),
                ExecutingTest = executingTest,
            };

            // If this specific DBContext can't use the Factory database key, then we use the DBContext specific key.
            dbConfig.ConnectionStringDatabaseKey = string.IsNullOrEmpty(connectionStringDatabaseKey)
                ? this.connectionStringDatabaseKey
                : connectionStringDatabaseKey;

            var seedBuilder = new DbContextSeedBuilder<TContext>(dbConfig);
            dbConfig.SeedBuilder = seedBuilder;
            this.contexts.Add(dbConfig);
            return seedBuilder;
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            foreach (TestDatabaseConfiguration dbConfig in this.contexts)
            {
                this.RenameConnectionStringDatabase(builder, dbConfig);
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

        protected virtual void RenameConnectionStringDatabase(IWebHostBuilder hostBuilder, TestDatabaseConfiguration databaseConfiguration)
        {
            hostBuilder.ConfigureAppConfiguration((hostContext, configBuilder) =>
            {
                IConfiguration configuration = configBuilder.Build();
                string connectionString = configuration.GetConnectionString(databaseConfiguration.ConfigurationConnectionStringKey);

                string timeStamp = DateTime.Now.ToString("HHmmss.fff");
                string dbName = this.GetDatabaseNameFromConnectionString(connectionString, databaseConfiguration.ConnectionStringDatabaseKey);
                string newDbName = $"Tests-{dbName}-{timeStamp}";
                connectionString = this.ReplaceDatabaseNameOnConnectionString(newDbName, connectionString, databaseConfiguration.ConnectionStringDatabaseKey);

                var connectionStringConfig = new Dictionary<string, string>()
                {
                    { databaseConfiguration.ConfigurationConnectionStringKey, connectionString }
                };

                configBuilder.AddInMemoryCollection(connectionStringConfig);
            });
        }

        protected virtual string GetDatabaseNameFromConnectionString(string connectionString, string connectionStringDatabaseKey)
        {
            Dictionary<string, string> connectionStringParts = this.GetConnectionStringParts(connectionString);
            if (connectionStringParts.TryGetValue(connectionStringDatabaseKey, out string key))
            {
                return connectionStringParts[connectionStringDatabaseKey];
            }

            throw new KeyNotFoundException($"The connection string database key of {connectionStringDatabaseKey} does not exist in the connection string.");
        }

        protected virtual string ReplaceDatabaseNameOnConnectionString(string newDbName, string connectionString, string connectionStringDatabaseKey)
        {
            Dictionary<string, string> connectionStringParts = this.GetConnectionStringParts(connectionString);
            if (!connectionStringParts.TryGetValue(connectionStringDatabaseKey, out string key))
            {
                throw new KeyNotFoundException($"The connection string database key of {connectionStringDatabaseKey} does not exist in the connection string.");
            }

            connectionStringParts[connectionStringDatabaseKey] = newDbName;

            var connectionStringBuilder = new DbConnectionStringBuilder();
            foreach(KeyValuePair<string, string> element in connectionStringParts)
            {
                connectionStringBuilder[element.Key] = element.Value;
            }

            return connectionStringBuilder.ToString();
        }

        private Dictionary<string, string> GetConnectionStringParts(string connectionString)
        {
            Dictionary<string, string> connectionStringParts = connectionString.Split(';')
                .Select(element => element.Split(new char[] { '=' }, 2))
                .ToDictionary(element => element[0].Trim(), element => element[1].Trim(), StringComparer.InvariantCultureIgnoreCase);

            return connectionStringParts;
        }
    }
}
