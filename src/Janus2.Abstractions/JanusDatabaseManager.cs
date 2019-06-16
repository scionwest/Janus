using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Janus
{
    public class JanusDatabaseManager : IDatabaseManager
    {
        private List<IDatabaseBuilder> databaseBuilders = new List<IDatabaseBuilder>();

        public void Reset()
        {
            this.databaseBuilders.Clear();
        }

        public IDatabaseSeedBuilder<TContext> ConfigureAndSeedDatabase<TContext>() where TContext : DbContext
        {
            return this.ConfigureDatabase<TContext>()
                .SeedDatabase();
        }

        public IDatabaseBuilder<TContext> ConfigureDatabase<TContext>() where TContext : DbContext
        {
            var builderOptions = new DatabaseBuilderOptions();
            IDatabaseBuilder<TContext> builder = new JanusDatabaseBuilder<TContext>(builderOptions);

            this.databaseBuilders.Add(builder);
            return builder;
        }

        public bool IsConfigured()
        {
            throw new NotImplementedException();
        }
    }

    public class JanusDatabaseBuilder<TContext> : IDatabaseBuilder<TContext> where TContext : DbContext
    {
        private IDatabaseSeedBuilder<TContext> seedBuilder;

        public JanusDatabaseBuilder(DatabaseBuilderOptions options) => this.Configuration = options;

        public DatabaseBuilderOptions Configuration { get; }

        public void Build()
        {
            throw new NotImplementedException();
        }

        public IDatabaseManager BuildDatabase()
        {
            throw new NotImplementedException();
        }

        public Type GetDatabaseContextType() => typeof(TContext);

        public IEntitySeeder[] GetSeeders()
        {
            return this.seedBuilder.RegisteredSeeders;
        }

        public IDatabaseSeedBuilder<TContext> SeedDatabase()
        {
            this.seedBuilder = new JanusDatabaseSeedBuilder<TContext>(this);
            return this.seedBuilder;
        }
    }

    public class JanusDatabaseSeedBuilder<TContext> : IDatabaseSeedBuilder<TContext> where TContext : DbContext
    {
        private IDatabaseBuilder<TContext> databaseBuilder;
        private List<IEntitySeeder<TContext>> seeders = new List<IEntitySeeder<TContext>>();

        public JanusDatabaseSeedBuilder(IDatabaseBuilder<TContext> databaseBuilder) => this.databaseBuilder = databaseBuilder;

        public IEntitySeeder[] RegisteredSeeders => seeders.ToArray();

        public IDatabaseManager BuildDatabase()
        {
            foreach(IEntitySeeder seeder in this.seeders)
            {
                seeder.Generate();
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
