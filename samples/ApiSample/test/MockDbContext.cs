using Microsoft.EntityFrameworkCore;

namespace AspNetCore.IntegrationTestSeeding
{
    public class MockDbContext : DbContext
    {
        public MockDbContext(DbContextOptions options) : base(options) { }
    }
}
