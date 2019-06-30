using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Janus.EntityFrameworkCore
{
    public class JanusDatabaseBuilder : IDatabaseBuilder
    {
        private readonly IDatabaseSeeder databaseSeeder;
        private List<DatabaseBuilderSetup> databaseBuilderSetups = new List<DatabaseBuilderSetup>();

        public JanusDatabaseBuilder(IDatabaseSeeder databaseSeeder)
        {
            this.databaseSeeder = databaseSeeder;
        }

        public DatabaseBuilderSetup DefaultSetup { get; set; }

        public IDatabaseSeeder AddContext<TContext>(Action<DatabaseBuilderSetup> setupDelegate)
        {
            Type dbContextType = typeof(TContext);
            bool isDbContext = typeof(DbContext).IsAssignableFrom(dbContextType);
            if (!isDbContext)
            {
                throw new InvalidDbContext(typeof(TContext));
            }

            var builderSetup = new DatabaseBuilderSetup(dbContextType);
            setupDelegate.Invoke(builderSetup);
            return this.databaseSeeder;
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
