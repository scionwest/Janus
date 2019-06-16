using Janus.Seeding;
using Microsoft.EntityFrameworkCore;
using System;

namespace Janus
{
    public interface IDatabaseManager : IDisposable
    {
        void Reset();

        bool IsConfigured();

        IDatabaseBuilder<TContext> ConfigureDatabase<TContext>() where TContext : DbContext;
        IDatabaseSeedBuilder<TContext> ConfigureAndSeedDatabase<TContext>() where TContext : DbContext;
        IDatabaseBuilder<TContext> ConfigureDatabase<TContext>(Func<string, string, string> databaseNamefactory) where TContext : DbContext;
    }
}
