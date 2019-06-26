using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Janus.EntityFrameworkCore
{
    internal class JanusDatabaseBuilderDelegate
    {
        internal Action<DatabaseBuilderSetup> SetupDelegate { get; set; }
        internal DatabaseBuilderSetup DatabaseSetup { get; set; }
    }

    public class JanusDatabaseBuilder : IDatabaseBuilder
    {
        private List<JanusDatabaseBuilderDelegate> databaseSetups = new List<JanusDatabaseBuilderDelegate>();

        public DatabaseBuildBehavior DefaultBehavior { get; set; }

        public IDatabaseBuilder AddContext<TContext>(Action<DatabaseBuilderSetup> setupDelegate)
        {
            Type dbContextType = typeof(TContext);
            bool isDbContext = typeof(DbContext).IsAssignableFrom(dbContextType);
            if (!isDbContext)
            {
                throw new InvalidDbContext(typeof(TContext));
            }

            var builderSetup = new DatabaseBuilderSetup(dbContextType);
            var builderDelegate = new JanusDatabaseBuilderDelegate
            {
                DatabaseSetup = builderSetup,
                SetupDelegate = setupDelegate,
            };

            return this;
        }

        public void Build()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //this.dbContext?.Database?.EnsureDeleted();
        }
    }
}
