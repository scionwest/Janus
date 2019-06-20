using Janus.Seeding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus.EntityFramework
{
    public class JanusDataContextSeeder : IDataContextSeeder
    {
        private readonly ISeedReaderFactory seedReaderFactory;

        public JanusDataContextSeeder(ISeedReaderFactory seedReaderFactory)
        {
            this.seedReaderFactory = seedReaderFactory;
        }

        public Dictionary<Type, object[]> SeedData { get; private set; } = new Dictionary<Type, object[]>();

        public void SeedDataContext<TContext>(TContext context, IEntitySeeder[] seeders) where TContext : DbContext
        {
            ISeedReader seedReader = this.seedReaderFactory.CreateReader(seeders);
            foreach (IEntitySeeder seeder in seeders)
            {
                seeder.Generate();
            }

            this.GenerateRelationships(seeders, seedReader);
            this.ValidateSeedData(seeders, seedReader);
            this.SeedData = this.GetAllSeedData(seeders);

            foreach (KeyValuePair<Type, object[]> seededData in this.SeedData)
            {
                context.AddRange(seededData.Value);
            }

            context.SaveChanges();
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
