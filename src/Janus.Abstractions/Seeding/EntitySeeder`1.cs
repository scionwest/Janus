using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus.Seeding
{
    public abstract class EntitySeeder<TEntity> : IEntitySeeder<TEntity>
    {
        public Type SeedType => typeof(TEntity);

        protected IList<TEntity> SeedData { get; private set; }

        public TEntity[] GetSeedEntities() => this.SeedData.ToArray();

        public object[] GetSeedData() => this.SeedData.Cast<object>().ToArray();

        public bool BuildRelationships(ISeedReader seedReader) => this.MapEntities(this.GetSeedEntities(), seedReader);

        public void Generate()
        {
            var seedOptions = new SeedOptions();
            this.SeedData = this.Seed(seedOptions);
        }

        public void ValidateSeedData(ISeedReader seedRead)
        {
            for (int index = 0; index < this.SeedData.Count; index++)
            {
                TEntity entity = this.SeedData[index];
                bool isValid = this.IsEntityValid(entity, seedRead);
                if (isValid)
                {
                    continue;
                }

                this.SeedData.RemoveAt(index);
            }
        }

        protected abstract bool MapEntities(TEntity[] seededEntities, ISeedReader seedReader);

        protected abstract IList<TEntity> Seed(SeedOptions options);

        protected virtual bool IsEntityValid(TEntity entity, ISeedReader seedReader) => true;
    }
}
