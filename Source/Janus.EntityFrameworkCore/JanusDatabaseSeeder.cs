using Janus.Seeding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Janus.EntityFrameworkCore
{
    public class JanusDatabaseSeeder : IDatabaseSeeder
    {
        public void SeedDataContext<TDbContext>(TDbContext dbContext, ISeedManager seedManager)
        {
            DbContext context = this.GetDbContext(dbContext);
            ISeedResult seedResult = seedManager.BuildSeedData();
            Dictionary<Type, object[]> seedData = seedResult.GetSeedData();

            foreach(KeyValuePair<Type, object[]> data in seedData)
            {
                context.AddRange(data.Value);
            }

            context.SaveChanges();
        }

        public void SeedDataContext<TDbContext>(TDbContext dbContext, IEntitySeeder entitySeeder)
        {
            DbContext context = this.GetDbContext(dbContext);
            object[] entities = entitySeeder.GetSeedData();
            context.AddRange(entities);
            context.SaveChanges();
        }

        public void SeedDataContext<TDbContext>(TDbContext dbContext, IEntitySeeder[] entitySeeders)
        {
            DbContext context = this.GetDbContext(dbContext);

            foreach (IEntitySeeder entitySeeder in entitySeeders)
            {
                object[] entities = entitySeeder.GetSeedData();
                context.AddRange(entities);
            }

            context.SaveChanges();
        }

        private DbContext GetDbContext<TDbContext>(TDbContext dbContext)
        {
            DbContext context = dbContext as DbContext ?? throw new InvalidDbContext(typeof(TDbContext));
            return context;
        }
    }
}
