using System;
using System.Collections.Generic;
using Janus.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Janus
{
    public class JanusDatabaseSeedBuilder<TContext> : IDatabaseSeedBuilder<TContext> where TContext : DbContext
    {
        private IDatabaseBuilder<TContext> databaseBuilder;
        private List<IEntitySeeder<TContext>> seeders = new List<IEntitySeeder<TContext>>();
        private readonly IDatabaseSeedReader seedReader;
        private readonly IDatabaseSeedWriter seedWriter;

        public JanusDatabaseSeedBuilder(IDatabaseSeedReader seedReader, IDatabaseSeedWriter seedWriter)
        {
            this.seedReader = seedReader;
            this.seedWriter = seedWriter;
        }

        public JanusDatabaseSeedBuilder(IDatabaseBuilder<TContext> databaseBuilder) => this.databaseBuilder = databaseBuilder;

        public IEntitySeeder[] RegisteredSeeders => seeders.ToArray();

        public IDatabaseManager BuildDatabase()
        {
            foreach(IEntitySeeder seeder in this.seeders)
            {
                seeder.Generate();
                object[] seededEntities = seeder.GetSeedData();
                this.seedWriter.WriteSeedToContext()
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
