using Microsoft.EntityFrameworkCore;

namespace Janus
{
    public class JanusDatabaseBuilderFactory : IDatabaseBuilderFactory
    {
        public IDatabaseBuilder<TContext> CreateDatabaseBuilder<TContext>() where TContext : DbContext
            => new JanusDatabaseBuilder<TContext>(new DatabaseBuilderOptions());
    }
}
