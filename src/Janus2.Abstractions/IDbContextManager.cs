using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Janus
{
    public interface IDatabaseManager
    {
        void Reset();

        bool IsConfigured();

        IDatabaseBuilder<TContext> ConfigureDatabase<TContext>() where TContext : DbContext;
        IDatabaseSeedBuilder<TContext> ConfigureAndSeedDatabase<TContext>() where TContext : DbContext;
    }

    public interface IDatabaseBuilder
    {
        DatabaseBuilderOptions Configuration { get; }
        Type GetDatabaseContextType();

        IEntitySeeder[] GetSeeders();
        IDatabaseManager BuildDatabase();
    }

    public interface IDatabaseBuilder<TContext> : IDatabaseBuilder where TContext : DbContext
    {
        IDatabaseSeedBuilder<TContext> SeedDatabase();
    }

    public interface IDatabaseSeedBuilder<TContext> where TContext : DbContext
    {
        IEntitySeeder[] RegisteredSeeders { get; }

        IDatabaseSeedBuilder<TContext> WithSeeder<TSeeder>() where TSeeder : IEntitySeeder, new();
        IDatabaseSeedBuilder<TContext> WithSeederCollection<TCollection>() where TCollection : IEntitySeederCollection;
        IDatabaseSeedBuilder<TContext> WithData(Action<TContext> entitySeeder);

        IDatabaseManager BuildDatabase();
    }

    public interface IDatabaseBuilderFactory
    {
        IDatabaseBuilder<TContext> CreateDatabaseBuilder<TContext>() where TContext : DbContext;
    }

    public class JanusDatabaseBuilderFactory : IDatabaseBuilderFactory
    {
        public IDatabaseBuilder<TContext> CreateDatabaseBuilder<TContext>() where TContext : DbContext
            => new JanusDatabaseBuilder<TContext>(new DatabaseBuilderOptions());
    }

    public class DatabaseBuilderOptions
    {
        public DatabaseBehavior DatabaseBehavior { get; set; } = DatabaseBehavior.AlwaysRetain;
        public ConnectionStringOptions ConnectionStringOptions { get; } = new ConnectionStringOptions();
        public DatabaseSeedOptions SeedConfiguration { get; } = new DatabaseSeedOptions();
    }

    public class DatabaseSeedOptions
    {
        public int DefaultCollectionSize { get; set; } = 100;
    }

    public class ConnectionStringOptions
    {
        public string DatabaseKey { get; set; } = "Initial Catalog";
        public string ConfigurationKey { get; set; } = "ConnectionStrings:Default";
    }

    [Flags]
    public enum DatabaseBehavior
    {
        GenerateOnSeed = 1,
        ResetOnSeed = 2,
        DeleteOnShutdown = 4,
        UniqueDatabasePerSeed = 8,
        AlwaysRetain = 16,
    }

    public interface IEntitySeederCollection
    {
    }

    public interface IEntitySeeder
    {
        Type SeedType { get; }
        void Generate();
        object[] GetSeedData();
        bool BuildRelationships(IDatabaseSeedReader seedRead);
        void ValidateSeedData(IDatabaseSeedReader seedRead);
    }

    public interface IEntitySeeder<TEntity> : IEntitySeeder
    {
        TEntity[] GetSeedEntities();
    }

    public interface IDatabaseSeedReader
    {
        TEntity[] GetSeededEntity<TEntity>();
    }

    public interface IDatabaseSeedWriter
    {
        void SaveSeedData(DbContext context, IEntitySeeder[] seeders);
    }
}
