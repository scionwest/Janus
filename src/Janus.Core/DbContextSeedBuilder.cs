using Janus.Seeding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus
{
    public class DbContextSeedBuilder
    {
        private Dictionary<Type, IEntitySeeder> registeredSeeders = new Dictionary<Type, IEntitySeeder>();

        internal DbContextSeedBuilder(TestDatabaseConfiguration databaseConfiguration)
        {
            this.TestConfiguration = databaseConfiguration ?? throw new ArgumentNullException(nameof(databaseConfiguration), $"You must pass in an instance of the {typeof(TestDatabaseConfiguration).Name}");
            databaseConfiguration.SeedBuilder = this;
        }

        internal TestDatabaseConfiguration TestConfiguration { get; }

        public IEntitySeeder[] GetEntitySeeders() 
            => this.registeredSeeders.Select(keyValue => keyValue.Value).ToArray();

        public DbContextSeedBuilder WithSeedData<TSeeder>() where TSeeder : IEntitySeeder, new()
        {
            if (this.registeredSeeders.ContainsKey(typeof(TSeeder)))
            {
                return this;
            }

            this.registeredSeeders.Add(typeof(TSeeder), new TSeeder());
            return this;
        }
    }
}
