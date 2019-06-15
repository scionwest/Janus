using Janus.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Janus
{
    public class ContextManager
    {
        internal const string DefaultDatabaseKey = "Initial Catalog";

        private readonly string connectionStringDatabaseKey;

        public ContextManager(string connectionStringDatabaseKey)
        {
            this.connectionStringDatabaseKey = string.IsNullOrEmpty(connectionStringDatabaseKey)
                ? DefaultDatabaseKey
                : connectionStringDatabaseKey;
        }

        internal List<TestDatabaseConfiguration> DatabaseConfigurations { get; } = new List<TestDatabaseConfiguration>();

        public IEntitySeeder GetDataContextSeedData<TContext, TEntitySeeder>() where TContext : DbContext where TEntitySeeder : IEntitySeeder
        {
            TestDatabaseConfiguration dbConfig = this.DatabaseConfigurations
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

        public DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, string newDbName = "") where TContext : DbContext
            => this.WithDataContext<TContext>(configurationConnectionStringKey, this.connectionStringDatabaseKey, newDbName);

        public DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, string connectionStringDatabaseKey, string newDbName = "") where TContext : DbContext
        {
            if (string.IsNullOrEmpty(configurationConnectionStringKey))
            {
                throw new ArgumentNullException(nameof(configurationConnectionStringKey), "You must provide the fully qualified IConfiguration Key used to discover the connection string value in the configuration system.");
            }

            var dbConfig = new TestDatabaseConfiguration<TContext>(configurationConnectionStringKey, newDbName)
            {
                // If this specific DBContext can't use the Factory database key, then we use the DBContext specific key.
                ConnectionStringDatabaseKey = string.IsNullOrEmpty(connectionStringDatabaseKey)
                    ? this.connectionStringDatabaseKey
                    : connectionStringDatabaseKey
            };

            var seedBuilder = new DbContextSeedBuilder<TContext>(dbConfig);
            this.DatabaseConfigurations.Add(dbConfig);
            return seedBuilder;
        }

        internal virtual void SeedDatabase(IServiceProvider services, TestDatabaseConfiguration databaseConfiguration, DbContext dbContext)
        {
            // Resolve a context seeder and seed the database with the individual seeders
            IDataContextSeeder contextSeeder = services.GetRequiredService<IDataContextSeeder>();
            IEntitySeeder[] entitySeeders = databaseConfiguration.SeedBuilder.GetEntitySeeders();
            contextSeeder.SeedDataContext(dbContext, entitySeeders);

            // Always run the callback seeder last so that individual tests have the ability to replace data
            // inserted via seeders.
            databaseConfiguration.DatabaseSeeder?.DynamicInvoke(dbContext);
        }
    }
}
