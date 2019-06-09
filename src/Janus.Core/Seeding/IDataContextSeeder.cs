using Microsoft.EntityFrameworkCore;

namespace Janus.Seeding
{
    public interface IDataContextSeeder
    {
        void SeedDataContext<TContext>(TContext context, IEntitySeeder[] seeders) where TContext : DbContext;
    }
}
