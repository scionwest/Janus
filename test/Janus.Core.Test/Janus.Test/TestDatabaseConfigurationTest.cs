using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Janus.Test
{
    [TestClass]
    public class TestDatabaseConfigurationTest
    {
        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        public void NonGenericConstructor_AssignsValuesProvided()
        {
            // Arrange
            string configKey = "ConnectionStrings:Default";
            Type contextType = typeof(DbContext);
            string testName = nameof(NonGenericConstructor_AssignsValuesProvided);

            // Act
            var dbConfig = new TestDatabaseConfiguration(configKey, contextType, testName);

            // Assert
            Assert.AreEqual(configKey, dbConfig.ConfigurationConnectionStringKey);
            Assert.AreEqual(contextType, dbConfig.DbContextType);
            Assert.AreEqual(testName, dbConfig.ExecutingTest);
        }

        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        public void GenericConstructor_AssignsValuesProvided()
        {
            // Arrange
            string configKey = "ConnectionStrings:Default";
            Type contextType = typeof(DbContext);
            string testName = nameof(NonGenericConstructor_AssignsValuesProvided);

            // Act
            var dbConfig = new TestDatabaseConfiguration<DbContext>(configKey, testName);

            // Assert
            Assert.AreEqual(configKey, dbConfig.ConfigurationConnectionStringKey);
            Assert.AreEqual(contextType, dbConfig.DbContextType);
            Assert.AreEqual(testName, dbConfig.ExecutingTest);
        }

        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonConstructor_WithNullConfigKey_Throws()
        {
            // Arrange
            string configKey = null;
            Type contextType = typeof(DbContext);
            string testName = nameof(NonGenericConstructor_AssignsValuesProvided);

            // Act
            _ = new TestDatabaseConfiguration(configKey, contextType, testName);
        }

        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonConstructor_WithNullContextType_Throws()
        {
            // Arrange
            string configKey = "ConnectionStrings:Default";
            Type contextType = null;
            string testName = nameof(NonGenericConstructor_AssignsValuesProvided);

            // Act
            _ = new TestDatabaseConfiguration(configKey, contextType, testName);
        }

        [TestMethod]
        [Owner("Johnathon Sullinger")]
        [TestCategory("WebFactory")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonConstructor_WithNullTestName_Throws()
        {
            // Arrange
            string configKey = "ConnectionStrings:Default";
            Type contextType = typeof(DbContext);
            string testName = null;

            // Act
            _ = new TestDatabaseConfiguration(configKey, contextType, testName);
        }
    }
}
