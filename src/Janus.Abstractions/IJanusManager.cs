using System;
using System.Collections.Generic;
using Janus.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Janus
{
    public interface IJanusManager
    {
        List<TestDatabaseConfiguration> DatabaseConfigurations { get; }

        IEntitySeeder GetDataContextSeedData<TContext, TEntitySeeder>()
            where TContext : DbContext
            where TEntitySeeder : IEntitySeeder;

        void SeedDatabase(IServiceProvider services, TestDatabaseConfiguration databaseConfiguration, DbContext dbContext);
        DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, string newDbName = "") where TContext : DbContext;
        DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, string connectionStringDatabaseKey, string newDbName = "") where TContext : DbContext;
    }
}