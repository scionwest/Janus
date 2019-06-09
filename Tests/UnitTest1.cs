using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using WebApplication1;

namespace AspNetCore.IntegrationTestSeeding
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            // Arrange
            var factory = new IntegrationTestSeedFactory<AppDbContext>()
                .ForDataContext<AppDbContext>("Default")
                .WithSeedData<AppDbContext><EmployeeSeeder>();

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/values");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
