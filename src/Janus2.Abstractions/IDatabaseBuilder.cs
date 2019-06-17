using Janus.Seeding;
using Microsoft.EntityFrameworkCore;
using System;

namespace Janus
{
    public interface IDatabaseBuilder : IDisposable
    {
        DatabaseBuilderOptions Configuration { get; }
        bool IsConfigured();
        Type GetDatabaseContextType();
        string GetDatabaseName(DatabaseNameKind nameKind);

        IEntitySeeder[] GetSeeders();
        IDatabaseManager BuildDatabase();
    }

    public enum DatabaseNameKind
    {
        Original,
        Unique
    }

    public interface IDatabaseBuilder<TContext> : IDatabaseBuilder where TContext : DbContext
    {
        IDatabaseSeedBuilder<TContext> SeedDatabase();
        IDatabaseBuilder<TContext> WithDatabaseName(Func<string, string, string> databaseNameFactory);
    }
}
