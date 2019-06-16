using Microsoft.EntityFrameworkCore;
using System;

namespace Janus.Seeding
{
    public interface IDatabaseSeedBuilder<TContext> where TContext : DbContext
    {
        IEntitySeeder[] RegisteredSeeders { get; }

        IDatabaseSeedBuilder<TContext> WithSeeder<TSeeder>() where TSeeder : IEntitySeeder, new();
        IDatabaseSeedBuilder<TContext> WithSeederCollection<TCollection>() where TCollection : IEntitySeederCollection;
        IDatabaseSeedBuilder<TContext> WithData(Action<TContext> entitySeeder);

        IDatabaseManager BuildDatabase();
    }

    public class JanusDatabaseSeedBuilder<TContext> : IDatabaseSeedBuilder<TContext> where TContext : DbContext
    {
        public IEntitySeeder[] RegisteredSeeders => throw new NotImplementedException();

        public IDatabaseManager BuildDatabase()
        {
            throw new NotImplementedException();
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
