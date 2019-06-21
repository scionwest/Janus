using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus.Seeding
{
    internal class JanusSeedResult : ISeedResult
    {
        internal JanusSeedResult(Dictionary<Type, object[]> seedData) => this.SeedData = seedData;

        protected Dictionary<Type, object[]> SeedData { get; }

        public Dictionary<Type, object[]> GetSeedData() => new Dictionary<Type, object[]>(this.SeedData);

        public TEntity[] GetSeedDataForEntity<TEntity>()
        {
            if (!this.SeedData.TryGetValue(typeof(TEntity), out object[] data))
            {
                throw new KeyNotFoundException($"THe {typeof(TEntity).Name} has not been registered to seed data with.");
            }

            return data.Cast<TEntity>().ToArray();
        }
    }
}
