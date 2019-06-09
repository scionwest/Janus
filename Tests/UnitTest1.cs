using AspNetCore.IntegrationTestSeeding;
using Bogus;
using Janus.Seeding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplication1;

namespace Janus
{
    [TestClass]
    public class UnitTest1
    {
        private Faker dataFaker;
        private ApiIntegrationTestFactory<Startup> testFactory;

        [TestInitialize]
        public void Initialize()
        {
            this.dataFaker = new Faker();
            this.testFactory = new ApiIntegrationTestFactory<Startup>("DataSource");
        }

        [TestCleanup]
        public void Cleanup() => this.testFactory.Dispose();

        [TestMethod]
        public async Task TestMethod1()
        {
            // Arrange
            var users = new UserEntity[]
            {
                new UserEntity { Address = this.dataFaker.Address.FullAddress(), Email = this.dataFaker.Internet.Email(), Username = this.dataFaker.Internet.UserName() },
                new UserEntity { Address = this.dataFaker.Address.FullAddress(), Email = this.dataFaker.Internet.Email(), Username = this.dataFaker.Internet.UserName() }
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
        [TestMethod]
        public async Task TestMethod2()
        {
            // Arrange
            this.testFactory.WithDataContext<AppDbContext>("Default")
                .WithSeedData<UserEntitySeeder>()
                .WithSeedData<TaskEntitySeeder>();

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
