using Janus.Seeding;

namespace Janus.EntityFrameworkCore
{
    public interface IDataContextSeeder
    {
        void SeedDataContext<TDbContext>(TDbContext context, ISeedManager seedManager);
        void SeedDataContext<TDbContext>(TDbContext context, IEntitySeeder entitySeeder);
        void SeedDataContext<TDbContext>(TDbContext context, IEntitySeeder[] entitySeeders);
    }
}
