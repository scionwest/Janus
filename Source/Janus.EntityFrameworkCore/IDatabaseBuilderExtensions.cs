using Microsoft.EntityFrameworkCore;
using System;

namespace Janus.EntityFrameworkCore
{
    public static class IDatabaseBuilderExtensions
    {
        public static void Build<TContext>(this IDatabaseBuilder<TContext> dbBuilder, Action<DatabaseConnectionStringInfo, DbContextOptionsBuilder<TContext>> contextBuilder) where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            contextBuilder(dbBuilder.ConnectionStringInfo, builder);

            TContext context = (TContext)Activator.CreateInstance(typeof(TContext), builder.Options);
            dbBuilder.Build(context);
        }
    }
}
