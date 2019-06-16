using Microsoft.EntityFrameworkCore;

namespace Janus
{
    public class TestDatabaseConfiguration<TContext> : TestDatabaseConfiguration where TContext : DbContext
    {
        internal TestDatabaseConfiguration(string configurationConnectionStringKey, string testName)
            : base(configurationConnectionStringKey, typeof(TContext), testName)
        {

        }
    }
}
