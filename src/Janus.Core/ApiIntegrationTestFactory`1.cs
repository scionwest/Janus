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
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Janus.Test")]

namespace Janus
{
    public class ApiIntegrationTestFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly ContextManager contextManager;
        private string solutionRelativeContentRoot = ".";

        public ApiIntegrationTestFactory() : this(ContextManager.DefaultDatabaseKey) {}

        public ApiIntegrationTestFactory(string connectionStringDatabaseKey)
        {
            this.contextManager = new ContextManager(connectionStringDatabaseKey);
        }

        public ApiIntegrationTestFactory<TStartup> SetSolutionRelativeContentRoot(string contentRoot = ".")
        {
            this.solutionRelativeContentRoot = contentRoot;
            return this;
        }

        public IEntitySeeder GetDataContextSeedData<TContext, TEntitySeeder>() where TContext : DbContext where TEntitySeeder : IEntitySeeder
        {
            return this.contextManager.GetDataContextSeedData<TContext, TEntitySeeder>();
        }

        public DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, [CallerMemberName] string executingTest = "") where TContext : DbContext
            => this.contextManager.WithDataContext<TContext>(configurationConnectionStringKey, executingTest);

        public DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, string connectionStringDatabaseKey, [CallerMemberName] string executingTest = "") where TContext : DbContext
        {
            return this.contextManager.WithDataContext<TContext>(configurationConnectionStringKey, connectionStringDatabaseKey, executingTest);
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            IWebHostBuilder builder = base.CreateWebHostBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IDataContextSeeder, DataContextSeeder>();
            });
            return builder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot(this.solutionRelativeContentRoot);
            base.ConfigureWebHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            foreach (TestDatabaseConfiguration dbConfig in this.contextManager.DatabaseConfigurations)
            {
                if (dbConfig.RetainDatabase)
                {
                    continue;
                }

                // These might be null in a suite of tests where some tests are configured with a Factory
                // in a [TestInitialize] method and some have a Factory created within the test itself.
                if (this.Server?.Host?.Services == null)
                {
                    continue;
                }

                DbContext context;
                using (IServiceScope serviceScope = this.Server.Host.Services.CreateScope())
                {
                    context = (DbContext)serviceScope.ServiceProvider.GetRequiredService(dbConfig.DbContextType);
                    context.Database.EnsureDeleted();
                }
            }

            base.Dispose(disposing);
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            foreach (TestDatabaseConfiguration dbConfig in this.contextManager.DatabaseConfigurations)
            {
                if (dbConfig.UseConfigurationDatabase)
                {
                    continue;
                }

                this.RenameConnectionStringDatabase(builder, dbConfig);
            }

            TestServer server = base.CreateServer(builder);
            this.InitializeDatabase(server);

            return server;
        }

        private void InitializeDatabase(TestServer server)
        {
            foreach (TestDatabaseConfiguration dbConfig in this.contextManager.DatabaseConfigurations)
            {
                DbContext context;
                using (IServiceScope serviceScope = server.Host.Services.CreateScope())
                {
                    try
                    {
                        context = (DbContext)serviceScope.ServiceProvider.GetRequiredService(dbConfig.DbContextType);
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new InvalidOperationException($"{dbConfig.DbContextType.FullName} has not been added to the services collection as a registered DbContext. Did you add it to your project using 'services.AddDbContext<{dbConfig.DbContextType.FullName}>'?", ex);
                    }

                    this.CreateDatabase(dbConfig, context);
                    this.contextManager.SeedDatabase(server.Host.Services, dbConfig, context);
                }
            }
        }

        protected virtual void CreateDatabase(TestDatabaseConfiguration configuration, DbContext context)
        {
            context.Database.EnsureCreated();
        }

        protected virtual void RenameConnectionStringDatabase(IWebHostBuilder hostBuilder, TestDatabaseConfiguration databaseConfiguration)
        {
            hostBuilder.ConfigureAppConfiguration((hostContext, configBuilder) =>
            {
                IConfiguration configuration = configBuilder.Build();
                string connectionString = configuration.GetConnectionString(databaseConfiguration.ConfigurationConnectionStringKey);
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = configuration[databaseConfiguration.ConfigurationConnectionStringKey];
                }

                string dbName = this.GetDatabaseNameFromConnectionString(connectionString, databaseConfiguration.ConnectionStringDatabaseKey);
                string newDbName = this.CreateDatabaseNameForTest(dbName, databaseConfiguration);
                connectionString = this.ReplaceDatabaseNameOnConnectionString(newDbName, connectionString, databaseConfiguration.ConnectionStringDatabaseKey);
                string configKey = $"ConnectionStrings:{databaseConfiguration.ConfigurationConnectionStringKey}";

                if (string.IsNullOrEmpty(configKey))
                {
                    configKey = databaseConfiguration.ConfigurationConnectionStringKey;
                }

                var connectionStringConfig = new Dictionary<string, string>()
                {
                    { configKey, connectionString }
                };

                configBuilder.AddInMemoryCollection(connectionStringConfig);
            });
        }

        protected virtual string CreateDatabaseNameForTest(string initialDatabaseName, TestDatabaseConfiguration testDatabaseConfiguration)
        {
            string timeStamp = DateTime.Now.ToString("HHmmss.fff");
            string newDbName = $"{initialDatabaseName}-{testDatabaseConfiguration.ExecutingTest}-{timeStamp}";
            return newDbName;
        }

        private string GetDatabaseNameFromConnectionString(string connectionString, string connectionStringDatabaseKey)
        {
            Dictionary<string, string> connectionStringParts = this.GetConnectionStringParts(connectionString);
            if (connectionStringParts.TryGetValue(connectionStringDatabaseKey, out string dbName))
            {
                return dbName;
            }

            throw new KeyNotFoundException($"The connection string database key of {connectionStringDatabaseKey} does not exist in the connection string.");
        }

        private string ReplaceDatabaseNameOnConnectionString(string newDbName, string connectionString, string connectionStringDatabaseKey)
        {
            Dictionary<string, string> connectionStringParts = this.GetConnectionStringParts(connectionString);
            if (!connectionStringParts.TryGetValue(connectionStringDatabaseKey, out _))
            {
                throw new KeyNotFoundException($"The connection string database key of {connectionStringDatabaseKey} does not exist in the connection string.");
            }

            connectionStringParts[connectionStringDatabaseKey] = newDbName;

            var connectionStringBuilder = new DbConnectionStringBuilder();
            foreach (KeyValuePair<string, string> element in connectionStringParts)
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
