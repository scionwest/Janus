using Janus.SampleApi;
using Janus.SampleApi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Janus
{
    public class JanusTestBuilder
    {
        internal Action<IDatabaseManager> BuilderCallback { get; set; }
    }

    public class JanusFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly DatabaseBuilderOptions databaseBuilderOptions;
        private readonly IDatabaseManager databaseManager;

        private Action<DatabaseBuilderOptions> databaseBuilderOptionsCallback;
        private Action<IDatabaseManager> databaseManagerSetup;

        public JanusFactory()
        {
            this.databaseManager = new JanusDatabaseManager();
            this.databaseBuilderOptionsCallback = this.ConfigureBuilderOptions;
            this.databaseManagerSetup = manager => {  };

            this.databaseBuilderOptions = new DatabaseBuilderOptions
            {
                DatabaseBehavior = DatabaseBehavior.UniqueDatabasePerSeed
            };
        }

        public JanusFactory(Action<DatabaseBuilderOptions> policy) : this()
        {
            this.databaseBuilderOptionsCallback = policy;
        }

        private void ConfigureBuilderOptions(DatabaseBuilderOptions options)
        {
            options.ConnectionStringOptions.ConfigurationKey = this.databaseBuilderOptions.ConnectionStringOptions.ConfigurationKey;
            options.ConnectionStringOptions.DatabaseKey = this.databaseBuilderOptions.ConnectionStringOptions.DatabaseKey;
            options.DatabaseBehavior = this.databaseBuilderOptions.DatabaseBehavior;
            options.SeedConfiguration.DefaultCollectionSize = this.databaseBuilderOptions.SeedConfiguration.DefaultCollectionSize;
        }

        public void WithDatabase(Action<IDatabaseManager> databaseSetup)
        {
            this.databaseManagerSetup = databaseSetup;
        }

        public void WithStartupDatabase()
        {
            this.databaseManagerSetup = manager =>
            {
                
            };
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            builder.ConfigureServices(this.AddJanusServices);
            if (this.databaseBuilderOptions.DatabaseBehavior.HasFlag(DatabaseBehavior.UniqueDatabasePerSeed))
            {
                // Rename the database within the connection string
            }

            TestServer server = base.CreateServer(builder);

            return server;
        }

        protected virtual void AddJanusServices(IServiceCollection services)
        {
            services.AddJanusDatabaseSeeding(this.databaseBuilderOptionsCallback);
        }
    }

    [TestClass]
    public class TestClass
    {
        [TestMethod]
        public void Test()
        {
            // Arrange
            var factory = new JanusFactory<Startup>();
            factory.WithDatabase(manager => manager.ConfigureAndSeedDatabase<AppDbContext>().WithSeeder<User2EntitySeeder>());
        }
    }

    //[TestClass]
    //public class CreateDbContextTests
    //{
    //    private Faker dataFaker;
    //    private JanusTestFactory<Startup> testFactory;

    //    [TestInitialize]
    //    public void Initialize()
    //    {
    //        this.dataFaker = new Faker();

    //        // Sqlite connection string uses "Data Source".
    //        // Replace with "Initial Catalog" for Sql Server or "database" for MySql.
    //        this.testFactory = new JanusTestFactory<Startup>("Data Source")
    //            .SetSolutionRelativeContentRoot("samples\\ApiSample\\src");

    //        this.testFactory
    //            .WithDataContext<AppDbContext>("Default")
    //            .WithSeedData<UserEntitySeeder>();
    //    }

    //    [TestCleanup]
    //    public void Cleanup()
    //    {
    //        // Destroy the database.
    //        this.testFactory.Dispose();
    //    }

    //    [TestMethod]
    //    [TestCategory("Samples")]
    //    public async Task GetUsers_ReturnsOkStatusCode()
    //    {
    //        // Arrange
    //        var client = this.testFactory.CreateClient();

    //        // Act
    //        HttpResponseMessage response = await client.GetAsync("api/users");

    //        // Assert
    //        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    //    }
    //}
}
