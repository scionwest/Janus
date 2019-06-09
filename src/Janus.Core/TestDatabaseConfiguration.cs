using System;

namespace Janus
{
    public class TestDatabaseConfiguration
    {
        public TestDatabaseConfiguration(string configurationConnectionStringKey, Type dbContextType, string testName)
        {
            if (string.IsNullOrEmpty(configurationConnectionStringKey))
            {
                throw new ArgumentNullException(nameof(configurationConnectionStringKey), "You must provide the fully qualified IConfiguration key, or a sub-key beneath the ConnectionStrings: Configuration Section.");
            }

            if (dbContextType == null)
            {
                throw new ArgumentNullException(nameof(dbContextType), "You can not provide a null DbContext Type.");
            }

            if (string.IsNullOrEmpty(testName))
            {
                throw new ArgumentNullException(nameof(testName), "You must specify the name of the test being executed for this configuration.");
            }

            this.ConfigurationConnectionStringKey = configurationConnectionStringKey;
            this.DbContextType = dbContextType;
            this.ExecutingTest = testName;
        }

        internal Type DbContextType { get; set; }
        internal string ConfigurationConnectionStringKey { get; set; }
        internal string ConnectionStringDatabaseKey { get; set; }
        internal bool RetainDatabase { get; set; }
        internal string ExecutingTest { get; set; }
        internal Delegate DatabaseSeeder { get; set; }
        internal DbContextSeedBuilder SeedBuilder { get; set; }
    }
}
