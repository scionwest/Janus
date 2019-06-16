using Janus.Seeding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus
{
    public class DbContextSeedBuilder
    {
        private readonly Dictionary<Type, IEntitySeeder> registeredSeeders = new Dictionary<Type, IEntitySeeder>();

        internal DbContextSeedBuilder(TestDatabaseConfiguration databaseConfiguration)
        {
            this.TestConfiguration = databaseConfiguration ?? throw new ArgumentNullException(nameof(databaseConfiguration), $"You must pass in an instance of the {typeof(TestDatabaseConfiguration).Name}");
            databaseConfiguration.SeedBuilder = this;
        }

        internal TestDatabaseConfiguration TestConfiguration { get; }

        public IEntitySeeder[] GetEntitySeeders() 
            => this.registeredSeeders.Select(keyValue => keyValue.Value).ToArray();

        public DbContextSeedBuilder WithSeedCollection<TCollection>() where TCollection : ISeederCollection, new()
        {
            TCollection collection = new TCollection();
            foreach(Type seeder in collection.GetSeederTypes())
            {
                this.WithSeedData(seeder);
            }
            return this;
        }

        public DbContextSeedBuilder WithSeedData<TSeeder>() where TSeeder : IEntitySeeder, new()
        {
            this.WithSeedData(typeof(TSeeder));
            return this;
        }

        private void WithSeedData(Type seederType)
        {
            if (this.registeredSeeders.ContainsKey(seederType))
            {
                return;
            }

            IEntitySeeder seeder = (IEntitySeeder)Activator.CreateInstance(seederType);
            this.registeredSeeders.Add(seederType, seeder);
        }

        public DbContextSeedBuilder RetainDatabase()
        {
            this.TestConfiguration.RetainDatabase = true;
            return this;
        }

        public DbContextSeedBuilder AlwaysRefreshDatabase()
        {
            this.TestConfiguration.GenerateFreshDatabase = true;
            return this;
        }

        public DbContextSeedBuilder UseConfiguredConnectionString()
        {
            this.TestConfiguration.UseConfigurationDatabase = true;
            return this;
        }
    }
}
