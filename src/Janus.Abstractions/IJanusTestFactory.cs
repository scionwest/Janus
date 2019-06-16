using Janus.Seeding;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Janus.Test")]

namespace Janus
{
    public interface IJanusTestFactory
    {
        IJanusTestFactory SetSolutionRelativeContentRoot(string contentRoot = ".");
        IEntitySeeder GetDataContextSeedData<TContext, TEntitySeeder>() 
            where TContext : DbContext 
            where TEntitySeeder : IEntitySeeder;

        DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, [CallerMemberName] string executingTest = "")
            where TContext : DbContext;
        DbContextSeedBuilder<TContext> WithDataContext<TContext>(string configurationConnectionStringKey, string connectionStringDatabaseKey, [CallerMemberName] string executingTest = "") 
            where TContext : DbContext;
    }
}
