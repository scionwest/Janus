using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus.Seeding
{
    public class JanusSeedManager : ISeedManager
    {
        private readonly ISeedReaderFactory seedReaderFactory;

        protected Dictionary<Type, IEntitySeeder> RegisteredSeeders { get; } = new Dictionary<Type, IEntitySeeder>();
        protected IEntitySeeder[] EntitySeeders => this.RegisteredSeeders.Select(item => item.Value).ToArray();

        public JanusSeedManager(ISeedReaderFactory seedReaderFactory) => this.seedReaderFactory = seedReaderFactory;

        public ISeedResult BuildSeedData()
        {
            if (this.RegisteredSeeders.Count == 0)
            {
                return new JanusSeedResult(new Dictionary<Type, object[]>());
            }

            foreach (IEntitySeeder seeder in EntitySeeders)
            {
                seeder.Generate();
            }

            ISeedReader seedReader = this.seedReaderFactory.CreateReader(EntitySeeders);
            this.GenerateRelationships(EntitySeeders, seedReader);
            this.ValidateSeedData(EntitySeeders, seedReader);
            Dictionary<Type, object[]> seedData = this.GetAllSeedData(EntitySeeders);

            return new JanusSeedResult(seedData);
        }

        public ISeedManager UseSeeder<TSeeder>() where TSeeder : IEntitySeeder, new()
        {
            var newSeeder = new TSeeder();
            return this.UseSeeder(newSeeder);
        }

        public ISeedManager UseSeeder(IEntitySeeder newSeeder)
        {
            Type seederType = newSeeder.GetType();
            if (this.RegisteredSeeders.TryGetValue(seederType, out IEntitySeeder _))
            {
                this.RegisteredSeeders[seederType] = newSeeder;
                return this;
            }

            this.RegisteredSeeders.Add(seederType, newSeeder);
            return this;
        }

        public ISeedManager UseSeeder(params IEntitySeeder[] entitySeeders)
        {
            foreach(IEntitySeeder newSeeder in entitySeeders)
            {
                this.UseSeeder(newSeeder);
            }

            return this;
        }

        private void GenerateRelationships(IEntitySeeder[] seeders, ISeedReader seedReader)
        {
            var incompleteSeeders = new HashSet<IEntitySeeder>(seeders);

            // Loop through the seeders until we've processed them all.
            while (incompleteSeeders.Count > 0)
            {
                int currentCount = incompleteSeeders.Count;
                foreach (IEntitySeeder seeder in incompleteSeeders.ToArray())
                {
                    bool isComplete = seeder.BuildRelationships(seedReader);
                    if (isComplete)
                    {
                        incompleteSeeders.Remove(seeder);
                    }
                }

                if (currentCount == incompleteSeeders.Count)
                {
                    break;
                }
            }
        }

        private void ValidateSeedData(IEntitySeeder[] seeders, ISeedReader seedReader)
        {
            foreach (IEntitySeeder seeder in seeders)
            {
                seeder.ValidateSeedData(seedReader);
            }
        }

        private Dictionary<Type, object[]> GetAllSeedData(IEntitySeeder[] seeders)
        {
            try
            {
                return seeders.ToDictionary(seeder => seeder.SeedType, seeder => seeder.GetSeedData());
            }
            catch (ArgumentException)
            {
                throw new DuplicateSeederException("Duplicate seeders have been used for seeding data. You must provide distinct seeders.");
            }
        }
    }
}
