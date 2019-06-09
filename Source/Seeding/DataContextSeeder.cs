using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus.Seeding
{
    public class DataContextSeeder : IDataContextSeeder, ISeedReader
    {
        private readonly ILogger<DataContextSeeder> logger;

        public DataContextSeeder(ILogger<DataContextSeeder> logger)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public Dictionary<Type, object[]> SeedData = new Dictionary<Type, object[]>();

        public TEntity[] GetDataForEntity<TEntity>()
        {
            if (!this.SeedData.TryGetValue(typeof(TEntity), out object[] data))
            {
                throw new KeyNotFoundException($"THe {typeof(TEntity).Name} has not been registered to seed data with.");
            }

            return data.Cast<TEntity>().ToArray();
        }

        public void SeedDataContext<TContext>(TContext context, IEntitySeeder[] seeders) where TContext : DbContext
        {
            this.GenerateData(seeders, context);
            this.GenerateRelationships(seeders);
            this.ValidateSeedData(seeders);

            foreach(KeyValuePair<Type, object[]> seededData in this.SeedData)
            {
                context.AddRange(seededData.Value);
            }

            context.SaveChanges();
        }

        private void GenerateData<TContext>(IEntitySeeder[] seeders, TContext dbContext) where TContext : DbContext
        {
            foreach (IEntitySeeder seeder in seeders)
            {
                seeder.Generate();
            }

            this.SeedData = this.GetAllSeedData(seeders);
        }

        private void GenerateRelationships(IEntitySeeder[] seeders)
        {
            var incompleteSeeders = new HashSet<IEntitySeeder>(seeders);

            // Loop through the seeders until we've processed them all.
            while(incompleteSeeders.Count > 0)
            {
                int currentCount = incompleteSeeders.Count;
                foreach(IEntitySeeder seeder in incompleteSeeders.ToArray())
                {
                    bool isComplete = seeder.BuildRelationships(this);
                    if (isComplete)
                    {
                        incompleteSeeders.Remove(seeder);
                    }
                }

                if (currentCount == incompleteSeeders.Count)
                {
                    this.logger.LogError("Unable to build relationships between all entities. Seeding of the data completed with incomplete relationships.");
                    break;
                }
            }
        }

        private void ValidateSeedData(IEntitySeeder[] seeders)
        {
            foreach(IEntitySeeder seeder in seeders)
            {
                seeder.ValidateSeedData(this);
            }

            // Refresh our seed data with the validated/sanitized seed data.
            this.SeedData = this.GetAllSeedData(seeders);
        }

        private Dictionary<Type, object[]> GetAllSeedData(IEntitySeeder[] seeders)
        {
            try
            {
                return seeders.ToDictionary(seeder => seeder.SeedType, seeder => seeder.GetSeedData());
            }
            catch(ArgumentException ex)
            {
                throw new DuplicateSeederException($"Duplicate seeders have been used for seeding data. You must provide distinct seeders. See inner exception for details on which seeder is a duplicate.", ex);
            }
        }
    }
}
