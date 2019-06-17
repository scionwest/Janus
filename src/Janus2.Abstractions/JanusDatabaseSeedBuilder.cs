using System;
using System.Collections.Generic;
using Janus.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Janus
{
    public class JanusDatabaseSeedBuilder<TContext> : IDatabaseSeedBuilder<TContext> where TContext : DbContext
    {
        private IDatabaseBuilder<TContext> databaseBuilder;
        private readonly IDatabaseSeedReader seedReader;
        private List<IEntitySeeder<TContext>> seeders = new List<IEntitySeeder<TContext>>();

        public JanusDatabaseSeedBuilder(IDatabaseBuilder<TContext> databaseBuilder, IDatabaseSeedReader seedReader)
        {
            this.databaseBuilder = databaseBuilder;
            this.seedReader = seedReader;
        }

        public IEntitySeeder[] RegisteredSeeders => seeders.ToArray();

        public IDatabaseManager BuildDatabase()
        {
            foreach(IEntitySeeder seeder in this.seeders)
            {
                seeder.Generate();
                object[] seededEntities = seeder.GetSeedData();
                seeder.BuildRelationships(this.seedReader);
            }

            return this.databaseBuilder.BuildDatabase();
        }

        public IDatabaseSeedBuilder<TContext> WithData(Action<TContext> entitySeeder)
        {
            throw new NotImplementedException();
        }

        public IDatabaseSeedBuilder<TContext> WithSeeder<TSeeder>() where TSeeder : IEntitySeeder, new()
        {
            throw new NotImplementedException();
        }

        public IDatabaseSeedBuilder<TContext> WithSeederCollection<TCollection>() where TCollection : IEntitySeederCollection
        {
            throw new NotImplementedException();
        }
    }
}
