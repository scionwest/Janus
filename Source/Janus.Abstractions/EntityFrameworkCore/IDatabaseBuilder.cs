using System;

namespace Janus.EntityFrameworkCore
{
    public interface IDatabaseBuilder : IDisposable
    {
        IDatabaseBuilder AddContext<TContext>(DatabaseBuildBehavior buildBehavior);
    }

    public interface IDatabaseManager : IDisposable
    {
        /// <summary>
        /// Builds all registered DbContexts
        /// </summary>
        void Build();

        // Builds a given instance of DbContext
        void Build<TContext>(TContext context);
    }
}
