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

namespace Janus
{
    public class ApiIntegrationTestFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private const string DefaultDatabaseKey = "Initial Catalog";

        private readonly string connectionStringDatabaseKey;
        private List<TestDatabaseConfiguration> databaseConfigurations = new List<TestDatabaseConfiguration>();

        public ApiIntegrationTestFactory() : this(DefaultDatabaseKey)
        { }

        public ApiIntegrationTestFactory(string connectionStringDatabaseKey)
        {
            this.connectionStringDatabaseKey = string.IsNullOrEmpty(connectionStringDatabaseKey)
                ? DefaultDatabaseKey
                : connectionStringDatabaseKey;
        }

        public IEntitySeeder GetDataContextSeedData<TContext, TEntitySeeder>() where TContext : DbContext where TEntitySeeder : IEntitySeeder
        {
            TestDatabaseConfiguration dbConfig = this.databaseConfigurations
                .First(config => config.DbContextType == typeof(TContext));

            if (dbConfig == null)
            {
                throw new InvalidOperationException($"The {typeof(TContext).FullName} DbContext is not registered with this factory instance. You can register it with {nameof(WithDataContext)}()");
            }

            IEntitySeeder entitySeeder = dbConfig.SeedBuilder
                .GetEntitySeeders()
                .FirstOrDefault(seeder => seeder.GetType() == typeof(TEntitySeeder));

            if (entitySeeder == null)
            {
                throw new InvalidOperationException($"The {typeof(TEntitySeeder).FullName} has not been registered for the DbContext {typeof(TContext).FullName}. You need to add the seeder to the context registration for this Factory instance.");
            }

            return entitySeeder;
        }

        public DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, [CallerMemberName] string executingTest = "") where TContext : DbContext
            => this.WithDataContext<TContext>(configurationConnectionStringKey, this.connectionStringDatabaseKey, executingTest);

        public DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, string connectionStringDatabaseKey, [CallerMemberName] string executingTest = "") where TContext : DbContext
        {
            if (string.IsNullOrEmpty(configurationConnectionStringKey))
            {
                throw new ArgumentNullException(nameof(configurationConnectionStringKey), "You must provide the fully qualified IConfiguration Key used to discover the connection string value in the configuration system.");
            }

            var dbConfig = new TestDatabaseConfiguration<TContext>(configurationConnectionStringKey, executingTest);

            // If this specific DBContext can't use the Factory database key, then we use the DBContext specific key.
            dbConfig.ConnectionStringDatabaseKey = string.IsNullOrEmpty(connectionStringDatabaseKey)
                ? this.connectionStringDatabaseKey
                : connectionStringDatabaseKey;

            var seedBuilder = new DbContextSeedBuilder<TContext>(dbConfig);
            this.databaseConfigurations.Add(dbConfig);
            return seedBuilder;
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
            builder.UseSolutionRelativeContentRoot(".");
            base.ConfigureWebHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            foreach (TestDatabaseConfiguration dbConfig in this.databaseConfigurations)
            {
                if (dbConfig.RetainDatabase)
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
            foreach (TestDatabaseConfiguration dbConfig in this.databaseConfigurations)
            {
                this.RenameConnectionStringDatabase(builder, dbConfig);
            }

            TestServer server = base.CreateServer(builder);
            this.InitializeDatabase(server);

            return server;
        }

        private void InitializeDatabase(TestServer server)
        {
            foreach (TestDatabaseConfiguration dbConfig in this.databaseConfigurations)
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
                    this.SeedDatabase(server, dbConfig, context);
                }
            }
        }

        protected virtual void CreateDatabase(TestDatabaseConfiguration configuration, DbContext context)
        {
            context.Database.EnsureCreated();
        }

        protected virtual void SeedDatabase(TestServer server, TestDatabaseConfiguration databaseConfiguration, DbContext dbContext)
        {
            // Resolve a context seeder and seed the database with the individual seeders
            IDataContextSeeder contextSeeder = server.Host.Services.GetRequiredService<IDataContextSeeder>();
            IEntitySeeder[] entitySeeders = databaseConfiguration.SeedBuilder.GetEntitySeeders();
            contextSeeder.SeedDataContext(dbContext, entitySeeders);

            // Always run the callback seeder last so that individual tests have the ability to replace data
            // inserted via seeders.
            databaseConfiguration.DatabaseSeeder?.DynamicInvoke(dbContext);
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
                string newDbName = this.CreateDatabaseNameForTest(dbName);
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

        protected virtual string CreateDatabaseNameForTest(string initialDatabaseName)
        {
            string timeStamp = DateTime.Now.ToString("HHmmss.fff");
            string newDbName = $"Tests-{initialDatabaseName}-{timeStamp}";
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
            if (!connectionStringParts.TryGetValue(connectionStringDatabaseKey, out string key))
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
