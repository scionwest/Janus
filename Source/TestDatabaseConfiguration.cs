using System;

namespace Janus
{
    public class TestDatabaseConfiguration
    {
        internal Type DbContextType { get; set; }
        internal string ConfigurationConnectionStringKey { get; set; }
        internal string ConnectionStringDatabaseKey { get; set; }
        internal bool RetainDatabase { get; set; }
        internal string ExecutingTest { get; set; }
        internal Delegate DatabaseSeeder { get; set; }
        internal DbContextSeedBuilder SeedBuilder { get; set; }
    }
}
