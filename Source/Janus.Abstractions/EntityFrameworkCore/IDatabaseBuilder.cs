using System;

namespace Janus.EntityFrameworkCore
{
    public interface IDatabaseBuilder : IDisposable
    {
        Type DbContextType { get; }
        void Build<TContext>(TContext context);
    }
}
