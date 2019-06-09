using Microsoft.EntityFrameworkCore;

namespace Janus.SampleApi.Data
{
    public class MockDbContext : DbContext
    {
        public MockDbContext(DbContextOptions options) : base(options) { }
    }
}
