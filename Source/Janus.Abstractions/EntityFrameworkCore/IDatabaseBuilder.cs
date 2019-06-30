using System;

namespace Janus.EntityFrameworkCore
{
    public interface IDatabaseBuilder : IDisposable
    {
        DatabaseBuilderSetup DefaultSetup { get; }

        /// <summary>
        /// Builds all registered DbContexts
        /// </summary>
        void Build();

        // Builds a given instance of DbContext
        IDatabaseSeeder AddContext<TContext>(Action<DatabaseBuilderSetup> setupDelegate);
    }

    public class DatabaseBuilderSetup
    {
        public DatabaseBuilderSetup(Type dbContextType)
        {
            this.DbContextType = dbContextType;
        }

        public Type DbContextType { get; }

        public string ConnectionStringKey { get; set; }

        public Func<string, string> ConnectionStringFormatter { get; set; }

        public DatabaseBuildBehavior BuildBehavior { get; set; } = DatabaseBuildBehavior.AlwaysRetain;

    }
}
