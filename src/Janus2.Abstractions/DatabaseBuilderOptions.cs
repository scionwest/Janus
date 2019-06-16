using Janus.Seeding;

namespace Janus
{
    public class DatabaseBuilderOptions
    {
        public DatabaseBehavior DatabaseBehavior { get; set; } = DatabaseBehavior.AlwaysRetain;
        public ConnectionStringOptions ConnectionStringOptions { get; } = new ConnectionStringOptions();
        public JanusDatabaseSeedOptions SeedConfiguration { get; } = new JanusDatabaseSeedOptions();
    }
}
