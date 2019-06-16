using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus.Seeding
{
    public abstract class EntitySeeder<TEntity> : IEntitySeeder<TEntity>
    {
        public Type SeedType => throw new NotImplementedException();

        public IList<TEntity> SeedData { get; private set; }

        public bool BuildRelationships(IDatabaseSeedReader seedReader) => this.MapEntities(this.GetSeedEntities(), seedReader);

        public void Generate()
        {
            var seedOptions = new JanusDatabaseSeedOptions();
            this.Seed(seedOptions);
        }

        public object[] GetSeedData() => this.SeedData.Cast<object>().ToArray();

        public TEntity[] GetSeedEntities() => this.SeedData.ToArray();

        public void ValidateSeedData(IDatabaseSeedReader seedReader)
        {
            for(int index = 0; index < this.SeedData.Count; index++)
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

        protected abstract bool MapEntities(TEntity[] seededEntities, IDatabaseSeedReader seedReader);

        protected abstract IList<TEntity> Seed(JanusDatabaseSeedOptions options);

        protected virtual bool IsEntityValid(TEntity entity, IDatabaseSeedReader seedReader) => true;
    }
}
