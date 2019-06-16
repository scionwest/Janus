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
    }
}
