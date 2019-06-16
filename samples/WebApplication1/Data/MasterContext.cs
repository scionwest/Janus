using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data
{
    public class MasterContext : DbContext
    {
        public MasterContext(DbContextOptions<MasterContext> options) : base(options) { }

        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<UserEntity> Users { get; set; }

        public DbSet<ProjectEntity> Projects { get; set; }
        public DbSet<TaskContext> Tasks { get; set; }
    }
}
