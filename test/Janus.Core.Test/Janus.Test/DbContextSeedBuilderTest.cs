using Janus.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Janus.Test
{
    [TestClass]
    public class DbContextSeedBuilderTest
    {
        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        public void Constructor_AssignsTestConfiguration()
        {
            // Arrange
            string configKey = "ConnectionStrings:Default";
            Type contextType = typeof(DbContext);
            string testName = nameof(Constructor_AssignsTestConfiguration);
            var dbConfig = new TestDatabaseConfiguration(configKey, contextType, testName);

            // Act
            var seedBuilder = new DbContextSeedBuilder(dbConfig);

            // Assert
            Assert.AreEqual(dbConfig, seedBuilder.TestConfiguration);
        }

        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        public void WithSeedData_ReturnsBuilder()
        {
            // Arrange
            string configKey = "ConnectionStrings:Default";
            Type contextType = typeof(DbContext);
            string testName = nameof(Constructor_AssignsTestConfiguration);
            var dbConfig = new TestDatabaseConfiguration(configKey, contextType, testName);

            // Act
            var seedBuilder = new DbContextSeedBuilder(dbConfig).WithSeedData<MockSeeder>();

            // Assert
            Assert.IsNotNull(seedBuilder);
        }

        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        public void GetEntitySeeders_ReturnsRegisteredSeeders()
        {
            // Arrange
            string configKey = "ConnectionStrings:Default";
            Type contextType = typeof(DbContext);
            string testName = nameof(Constructor_AssignsTestConfiguration);
            var dbConfig = new TestDatabaseConfiguration(configKey, contextType, testName);

            // Act
            var seedBuilder = new DbContextSeedBuilder(dbConfig).WithSeedData<MockSeeder>();
            IEntitySeeder[] seeders = seedBuilder.GetEntitySeeders();

            // Assert
            Assert.AreEqual(1, seeders.Length);
            Assert.AreEqual(typeof(MockSeeder), seeders.First().GetType());
        }

        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        public void WithSeedData_AvoidsDuplicateRegistrations()
        {
            // Arrange
            string configKey = "ConnectionStrings:Default";
            Type contextType = typeof(DbContext);
            string testName = nameof(Constructor_AssignsTestConfiguration);
            var dbConfig = new TestDatabaseConfiguration(configKey, contextType, testName);

            // Act
            var seedBuilder = new DbContextSeedBuilder(dbConfig)
                .WithSeedData<MockSeeder>()
                .WithSeedData<MockSeeder>();
            IEntitySeeder[] seeders = seedBuilder.GetEntitySeeders();

            // Assert
            Assert.AreEqual(1, seeders.Length);
        }

        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        public void WithSeedData_AssignsSeederCallback()
        {
            // Arrange
            string configKey = "ConnectionStrings:Default";
            Type contextType = typeof(DbContext);
            string testName = nameof(Constructor_AssignsTestConfiguration);
            var dbConfig = new TestDatabaseConfiguration(configKey, contextType, testName);
            Action<DbContext> callback = context => { };

            // Act
            var seedBuilder = new DbContextSeedBuilder<DbContext>(dbConfig)
                .WithSeedData(callback);

            // Assert
            Assert.AreEqual(callback, seedBuilder.TestConfiguration.DatabaseSeeder);
        }

        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        public void RetainDatabase_UpdatesTestConfiguration()
        {
            // Arrange
            string configKey = "ConnectionStrings:Default";
            Type contextType = typeof(DbContext);
            string testName = nameof(Constructor_AssignsTestConfiguration);
            var dbConfig = new TestDatabaseConfiguration(configKey, contextType, testName);

            // Act
            var seedBuilder = new DbContextSeedBuilder<DbContext>(dbConfig)
                .RetainDatabase();

            // Assert
            Assert.IsTrue(seedBuilder.TestConfiguration.RetainDatabase);
        }

        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConfiguration_Throws()
        {
            // Arrange
            TestDatabaseConfiguration dbConfig = null;

            // Act
            _ = new DbContextSeedBuilder(dbConfig);

            // Assert
            Assert.Fail("Expected exception to be thrown.");
        }
    }
}
