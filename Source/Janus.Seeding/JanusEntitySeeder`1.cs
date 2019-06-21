using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus.Seeding
{
    public abstract class JanusEntitySeeder<TEntity> : IEntitySeeder<TEntity>
    {
        public Type SeedType { get; } = typeof(TEntity);

        public bool IsSeeded { get; private set; }

        public IList<TEntity> SeedData { get; private set; } = new List<TEntity>();

        public object[] GetSeedData()
        {
            var data = new object[this.SeedData.Count];
            for(int index = 0; index < this.SeedData.Count; index++)
            {
                data[index] = this.SeedData[index];
            }

            return data;
        }

        public TEntity[] GetSeedEntities() => this.SeedData.ToArray();

        public bool BuildRelationships(ISeedReader seedReader) => this.MapEntities(this.GetSeedEntities(), seedReader);

        public void Generate()
        {
            var seedOptions = new JanusSeedOptions();
            this.SeedData = this.Seed(seedOptions);

            this.IsSeeded = true;
        }

        public void ValidateSeedData(ISeedReader seedReader)
        {
            for (int index = 0; index < this.SeedData.Count; index++)
            {
                TEntity entity = this.SeedData[index];
                bool isValid = this.IsEntityValid(entity, seedReader);
                if (isValid)
                {
                    continue;
                }

                // Not valid - don't keep it.
                this.SeedData.RemoveAt(index);
            }
        }

        protected virtual bool IsEntityValid(TEntity entity, ISeedReader seedReader) => true;

        protected abstract bool MapEntities(TEntity[] seededEntities, ISeedReader seedReader);

        protected abstract IList<TEntity> Seed(JanusSeedOptions options);
    }
}
