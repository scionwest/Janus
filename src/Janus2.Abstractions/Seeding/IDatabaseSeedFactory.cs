using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Janus.Seeding
{
    public interface IDatabaseSeedFactory
    {
        IDatabaseSeedBuilder<TContext> CreateSeedBuilder<TContext>() where TContext : DbContext;
        IDatabaseSeedBuilder<TContext> CreateSeedBuilder<TContext>(IDatabaseSeedReader seedReader, IDatabaseSeedWriter seedWriter) where TContext : DbContext;
        IDatabaseSeedReader CreateSeedReader();
        IDatabaseSeedWriter CreateSeedWriter();
    }

    public class DatabaseSeedFactory : IDatabaseSeedFactory
    {
        public IDatabaseSeedBuilder<TContext> CreateSeedBuilder<TContext>() where TContext : DbContext
            => this.CreateSeedBuilder<TContext>(this.CreateSeedReader(), this.CreateSeedWriter());

        public IDatabaseSeedBuilder<TContext> CreateSeedBuilder<TContext>(IDatabaseSeedReader seedReader, IDatabaseSeedWriter seedWriter) where TContext : DbContext
            => new JanusDatabaseSeedBuilder<TContext>(seedReader, seedWriter);

        public IDatabaseSeedReader CreateSeedReader()
        {
            throw new NotImplementedException();
        }

        public IDatabaseSeedWriter CreateSeedWriter()
        {
            throw new NotImplementedException();
        }
    }

    public class JanusDatabaseSeedReader<TContext> : IDatabaseSeedReader where TContext : DbContext
    {
        private readonly IDatabaseSeedBuilder<TContext> seedBuilder;

        public JanusDatabaseSeedReader(IDatabaseSeedBuilder<TContext> seedBuilder)
        {
            this.seedBuilder = seedBuilder;
        }

        public Dictionary<Type, object[]> SeedData = new Dictionary<Type, object[]>();

        public TEntity[] GetSeededEntities<TEntity>()
        {
            if (!this.SeedData.TryGetValue(typeof(TEntity), out object[] data))
            {
                throw new KeyNotFoundException($"The {typeof(TEntity).Name} has not been registered to seed data with.");
            }

            return data.Cast<TEntity>().ToArray();
        }
    }
}
