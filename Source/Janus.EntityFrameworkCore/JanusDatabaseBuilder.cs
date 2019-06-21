using Microsoft.EntityFrameworkCore;
using System;

namespace Janus.EntityFrameworkCore
{
    public class JanusDatabaseBuilder<TContext> : IDatabaseBuilder<TContext> where TContext : DbContext
    {
        private TContext dbContext;

        public JanusDatabaseBuilder(string connectionString)
        {
            this.ConnectionStringInfo = new DatabaseConnectionStringInfo(connectionString, new JanusConnectionStringOptions());
            this.DbContextType = typeof(TContext);
        }

        public JanusDatabaseBuilder(DatabaseConnectionStringInfo connectionStringInfo)
        {
            this.ConnectionStringInfo = connectionStringInfo;
            this.DbContextType = typeof(TContext);
        }

        public Type DbContextType { get; }
        public DatabaseConnectionStringInfo ConnectionStringInfo { get; private set; }

        public void Build(TContext context)
        {
            this.dbContext = context;
            context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            this.dbContext.Database.EnsureDeleted();
        }

        public IDatabaseBuilder<TContext> WithUseCase(string useCase)
        {
            this.ConnectionStringInfo = new DatabaseConnectionStringInfo(this.ConnectionStringInfo.ConnectionString, useCase, new JanusConnectionStringOptions());
            return this;
        }
    }
}
