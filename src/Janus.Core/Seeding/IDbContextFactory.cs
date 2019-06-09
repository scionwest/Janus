using Microsoft.EntityFrameworkCore;

namespace Janus.Seeding
{
    public interface IDbContextFactory
    {
        TContext CreateDbContext<TContext>() where TContext : DbContext, new();
    }
}
