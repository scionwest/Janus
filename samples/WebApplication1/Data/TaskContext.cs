using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data
{
    public class TaskContext : DbContext
    {
        public TaskContext(DbContextOptions<TaskContext> options) : base(options) { }

        public DbSet<ProjectEntity> Projects { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }
    }
}
