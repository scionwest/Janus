using Microsoft.EntityFrameworkCore;
using System;

namespace Janus.SampleApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<TaskEntity> Tasks { get; set; }
    }
}
