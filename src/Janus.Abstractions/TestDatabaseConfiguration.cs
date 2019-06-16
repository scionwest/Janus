using System;

namespace Janus
{
    public class TestDatabaseConfiguration
    {
        private bool useConfigurationConnectionString = false;
        private bool retainDatabase = false;

        public TestDatabaseConfiguration(string configurationConnectionStringKey, Type dbContextType, string testName)
        {
            if (string.IsNullOrEmpty(configurationConnectionStringKey))
            {
                throw new ArgumentNullException(nameof(configurationConnectionStringKey), "You must provide the fully qualified IConfiguration key, or a sub-key beneath the ConnectionStrings: Configuration Section.");
            }

            if (testName == null)
            {
                testName = string.Empty;
            }

            this.ConfigurationConnectionStringKey = configurationConnectionStringKey;
            this.DbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType), "You can not provide a null DbContext Type.");
            this.ExecutingTest = testName;
        }

        public bool GenerateFreshDatabase { get; internal set; }
        public Type DbContextType { get; set; }
        public string ConfigurationConnectionStringKey { get; set; }
        public string ConnectionStringDatabaseKey { get; set; }
        public string ExecutingTest { get; set; }
        public Delegate DatabaseSeeder { get; set; }
        public DbContextSeedBuilder SeedBuilder { get; set; }

        public bool RetainDatabase
        {
            get
            {
                return this.retainDatabase;
            }
            set
            {
                this.retainDatabase = value;
                if (!value)
                {
                    // Do not allow UseConfigurationDatabase to be true if you opt to not retain databases.
                    this.UseConfigurationDatabase = false;
                }
            }
        }

        public bool UseConfigurationDatabase
        {
            get
            {
                return this.useConfigurationConnectionString;
            }
            set
            {
                this.useConfigurationConnectionString = value;
                if (value)
                {
                    // Do not allow RetainDatabase to be false if you opt to use the connection string defined by
                    // the applications TStartup configuration.
                    this.RetainDatabase = true;
                }
            }
        }
    }
}
