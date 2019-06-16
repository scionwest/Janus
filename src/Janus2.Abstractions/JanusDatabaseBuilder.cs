using System;
using Janus.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Janus
{
    public class JanusDatabaseBuilder<TContext> : IDatabaseBuilder<TContext> where TContext : DbContext
    {
        private IDatabaseSeedBuilder<TContext> seedBuilder;
        private bool isDatabaseBuilt = false;

        public JanusDatabaseBuilder(DatabaseBuilderOptions options, IDatabaseSeedReader seedReader, IDatabaseSeedWriter seedWriter)
        {
            this.Configuration = options;
        }

        public DatabaseBuilderOptions Configuration { get; }

        public bool IsConfigured()
        {
            return isDatabaseBuilt;
        }

        public void Build()
        {
            throw new NotImplementedException();
        }

        public IDatabaseManager BuildDatabase()
        {
            throw new NotImplementedException();
            this.isDatabaseBuilt = true;
        }

        public Type GetDatabaseContextType() => typeof(TContext);

        public string GetDatabaseName(DatabaseNameKind nameKind)
        {
            throw new NotImplementedException();
        }

        public IEntitySeeder[] GetSeeders()
        {
            return this.seedBuilder.RegisteredSeeders;
        }

        public IDatabaseSeedBuilder<TContext> SeedDatabase()
        {
            this.seedBuilder = new JanusDatabaseSeedBuilder<TContext>(this);
            return this.seedBuilder;
        }

        public IDatabaseBuilder<TContext> WithDatabaseName(Func<string, string, string> databaseNameFactory)
        {
            this.Configuration.ConnectionStringOptions.DatabaseNameFactory = databaseNameFactory;
            return this;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
