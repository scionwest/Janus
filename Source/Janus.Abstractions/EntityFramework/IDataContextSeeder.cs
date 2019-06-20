using Janus.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Janus.EntityFramework
{
    public interface IDataContextSeeder
    {
        void SeedDataContext<TContext>(TContext context, IEntitySeeder[] seeders) where TContext : DbContext;
    }
}
