using System;
using System.Linq;

namespace Janus.Seeding
{
    public class JanusSeedReader : ISeedReader
    {
        private readonly IEntitySeeder[] seeders;

        public JanusSeedReader(IEntitySeeder[] entitySeeders) => this.seeders = entitySeeders;

        public TEntity[] GetSeededEntities<TEntity>()
        {
            Type entityType = typeof(TEntity);
            return this.seeders
                .FirstOrDefault(seeder => seeder.SeedType == entityType)?
                .GetSeedData()?
                .Cast<TEntity>()?
                .ToArray() ?? Array.Empty<TEntity>();
        }
    }
}
