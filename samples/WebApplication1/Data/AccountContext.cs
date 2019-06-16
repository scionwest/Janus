using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data
{
    public class AccountContext : DbContext
    {
        public AccountContext(DbContextOptions<AccountContext> options) : base(options) { }

        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<UserEntity> Users { get; set; }
    }
}
