using Microsoft.EntityFrameworkCore;

namespace Janus.Seeding
{
    public interface IDatabaseSeedWriter
    {
        void WriteSeedToContext(DbContext context, IEntitySeeder[] seeders);
    }
}
