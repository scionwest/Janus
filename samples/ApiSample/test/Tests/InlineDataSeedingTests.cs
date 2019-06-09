using Bogus;
using Janus.SampleApi.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Janus.SampleApi
{
    [TestClass]
    public class InlineDataSeedingTests
    {
        private Faker dataFaker;
        private ApiIntegrationTestFactory<Startup> testFactory;

        [TestInitialize]
        public void Initialize()
        {
            this.dataFaker = new Faker();

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
        public async Task GetUsers_ReturnsFakeUsers()
        {
            // Arrange
            var users = new UserEntity[]
            {
                new UserEntity
                {
                    Address = this.dataFaker.Address.FullAddress(),
                    Email = this.dataFaker.Internet.Email(),
                    Username = this.dataFaker.Internet.UserName()
                },
                new UserEntity
                {
                    Address = this.dataFaker.Address.FullAddress(),
                    Email = this.dataFaker.Internet.Email(),
                    Username = this.dataFaker.Internet.UserName()
                },
            };

            this.testFactory.WithDataContext<AppDbContext>("Default")
                .WithSeedData(context =>
                {
                    context.Users.AddRange(users);
                    context.SaveChanges();
                });

            var client = this.testFactory.CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("api/users");
            string responseBody = await response.Content.ReadAsStringAsync();
            UserEntity[] responseData = JsonConvert.DeserializeObject<UserEntity[]>(responseBody);


            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(users.Length, responseData.Length);
        }
    }
}
