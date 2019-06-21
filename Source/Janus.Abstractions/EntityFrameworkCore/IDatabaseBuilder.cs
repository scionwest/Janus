using System;

namespace Janus.EntityFrameworkCore
{
    public interface IDatabaseBuilder<TContext> : IDisposable
    {
        Type DbContextType { get; }
        DatabaseConnectionStringInfo ConnectionStringInfo { get; }
        void Build(TContext context);
    }
}
