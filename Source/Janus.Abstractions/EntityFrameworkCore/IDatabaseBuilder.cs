using System;

namespace Janus.EntityFrameworkCore
{
    public interface IDatabaseBuilder : IDisposable
    {
        DatabaseBuildBehavior DefaultBehavior { get; set; }

        /// <summary>
        /// Builds all registered DbContexts
        /// </summary>
        void Build();

        // Builds a given instance of DbContext
        IDatabaseBuilder AddContext<TContext>(Action<DatabaseBuilderSetup> setupDelegate);
    }

    public class DatabaseBuilderSetup
    {
        public DatabaseBuilderSetup(Type dbContextType)
        {
            this.DbContextType = dbContextType;
        }

        public Type DbContextType { get; }

        public string ConnectionString { get; set; }

        public string ConnectionStringKey { get; set; }

        public DatabaseBuildBehavior BuildBehavior { get; set; }


    }
}
