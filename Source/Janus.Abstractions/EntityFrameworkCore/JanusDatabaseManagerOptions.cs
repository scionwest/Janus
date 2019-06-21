namespace Janus.EntityFrameworkCore
{
    public class JanusDatabaseManagerOptions
    {
        public DatabaseBuildBehavior Behavior { get; set; } = DatabaseBuildBehavior.AlwaysRetain;
        public JanusConnectionStringOptions ConnectionStringOptions { get; } = new JanusConnectionStringOptions();
    }
}
