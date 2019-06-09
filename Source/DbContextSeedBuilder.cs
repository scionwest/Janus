using Janus.Seeding;
using System.Collections.Generic;

namespace Janus
{
    public class DbContextSeedBuilder
    {
        private List<IEntitySeeder> registeredSeeders = new List<IEntitySeeder>();

        internal DbContextSeedBuilder(TestDatabaseConfiguration databaseConfiguration)
        {
            this.TestConfiguration = databaseConfiguration;
            databaseConfiguration.SeedBuilder = this;
        }

        internal TestDatabaseConfiguration TestConfiguration { get; }

        public IEntitySeeder[] GetEntitySeeders() => this.registeredSeeders.ToArray();

        public DbContextSeedBuilder WithSeedData<TSeeder>() where TSeeder : IEntitySeeder, new()
        {
            this.registeredSeeders.Add(new TSeeder());
            return this;
        }
    }
}
