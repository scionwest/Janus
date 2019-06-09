using Janus.Seeding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

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

    public class DbContextSeedBuilder
    {
        private List<IEntitySeeder> registeredSeeders = new List<IEntitySeeder>();

        internal DbContextSeedBuilder(TestDatabaseConfiguration databaseConfiguration) => this.TestConfiguration = databaseConfiguration;

        internal TestDatabaseConfiguration TestConfiguration { get; }

        public IEntitySeeder[] GetEntitySeeders() => this.registeredSeeders.ToArray();

        public DbContextSeedBuilder WithSeedData<TSeeder>() where TSeeder : IEntitySeeder, new()
        {
            this.registeredSeeders.Add(new TSeeder());
            return this;
        }
    }
}
