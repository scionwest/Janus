using Microsoft.EntityFrameworkCore;
using System;

namespace Janus
{
    public class DbContextSeedBuilder<TContext> : DbContextSeedBuilder where TContext : DbContext
    {
        internal DbContextSeedBuilder(TestDatabaseConfiguration databaseConfiguration) : base(databaseConfiguration) { }

        public DbContextSeedBuilder<TContext> WithSeedData(Action<TContext> seeder)
        {
            base.TestConfiguration.DatabaseSeeder = seeder;
            return this;
        }


        public DbContextSeedBuilder<TContext> RetainDatabase()
        {
            base.TestConfiguration.RetainDatabase = true;
            return this;
        }

        public DbContextSeedBuilder<TContext> UseConfiguredConnectionString()
        {
            base.TestConfiguration.UseConfigurationDatabase = true;
            return this;
        }
    }
}
