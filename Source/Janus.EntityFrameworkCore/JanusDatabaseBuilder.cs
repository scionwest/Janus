using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Janus.EntityFrameworkCore
{
    public class JanusDatabaseBuilder : IDatabaseBuilder
    {
        private Dictionary<Type, DbContext> dbContexts = new Dictionary<Type, DbContext>();

        public void Build<TContext>(TContext context)
        {
            if (this.dbContexts.TryGetValue(typeof(TContext), out DbContext existingContext))
            {
                throw new InvalidOperationException($"This Database Builder instance has already built {typeof(TContext).Name}.");
            }

            var dbContext = context as DbContext ?? throw new InvalidDbContext(typeof(TContext));
            dbContext.Database.EnsureCreated();
            this.dbContexts.Add(typeof(TContext), dbContext);
        }

        public void Dispose()
        {
            this.dbContext?.Database?.EnsureDeleted();
        }
    }
}
