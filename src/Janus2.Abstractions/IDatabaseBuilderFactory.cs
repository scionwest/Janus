using Microsoft.EntityFrameworkCore;

namespace Janus
{
    public interface IDatabaseBuilderFactory
    {
        IDatabaseBuilder<TContext> CreateDatabaseBuilder<TContext>() where TContext : DbContext;
    }
}
