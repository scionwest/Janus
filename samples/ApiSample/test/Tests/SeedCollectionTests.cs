using Bogus;
using Janus.SampleApi.Data;
using Janus.Seeding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Janus.SampleApi
{
    [TestClass]
    public class SeedCollectionTests
    {
        private ApiIntegrationTestFactory<Startup> testFactory;

        [TestInitialize]
        public void Initialize()
        {
            // Sqlite connection string uses "Data Source".
            // Replace with "Initial Catalog" for Sql Server or "database" for MySql.
            this.testFactory = new ApiIntegrationTestFactory<Startup>("Data Source")
                .SetSolutionRelativeContentRoot("samples\\ApiSample\\src");
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Destroy the database.
            this.testFactory.Dispose();
        }

        [TestMethod]
        [TestCategory("Samples")]
        public async Task GetUsers_ReturnsSeederData()
        {
            // Arrange
            this.testFactory.WithDataContext<AppDbContext>("Default")
                .WithSeedCollection<UserTaskCollection>();

            var client = testFactory.CreateClient();
            IEntitySeeder userSeeder = testFactory.GetDataContextSeedData<AppDbContext, UserEntitySeeder>();

            // Act
            HttpResponseMessage response = await client.GetAsync("api/users");
            string responseBody = await response.Content.ReadAsStringAsync();
            UserEntity[] responseData = JsonConvert.DeserializeObject<UserEntity[]>(responseBody);


            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(userSeeder.GetSeedData().Length, responseData.Length);
            Assert.IsTrue(responseData.Any(user => user.Tasks.Count > 0));
        }
    }
}
