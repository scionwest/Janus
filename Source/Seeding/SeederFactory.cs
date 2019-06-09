using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Janus.Seeding
{
    public class SeederFactory : ISeederFactory
    {
        public IEntitySeeder[] CreateSeeders()
        {
            Type entitySeederType = typeof(IEntitySeeder);
            Func<Type, bool> seederPredicate = (type) => !type.IsAbstract && entitySeederType.IsAssignableFrom(type) && entitySeederType.IsClass;

            // Get seeders from our own assembly
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Type[] localSeeders = currentAssembly
                .GetExportedTypes()
                .Where(seederPredicate)
                .ToArray();

            // Get seeders from all assemblies referenced by us
            Type[] referencedSeeders = currentAssembly
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(seederPredicate)
                .ToArray();

            // Get seeders from the calling assembly
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            Type[] callerSeeders = callingAssembly == currentAssembly
                ? Array.Empty<Type>()
                : callingAssembly.GetExportedTypes().Where(seederPredicate).ToArray();

            // Get seeders from all of the assemblies associated with the calling assembly
            Type[] dependentSeeders = Assembly.GetCallingAssembly()
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Where(assembly => assembly != currentAssembly)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(seederPredicate)
                .ToArray();

            Func<Type, IEntitySeeder> seederFactory = (type) => (IEntitySeeder)Activator.CreateInstance(type);
            var allSeeders = new List<IEntitySeeder>();

            allSeeders.AddRange(localSeeders.Select(seederFactory));
            allSeeders.AddRange(referencedSeeders.Select(seederFactory));
            allSeeders.AddRange(dependentSeeders.Select(seederFactory));
            allSeeders.AddRange(callerSeeders.Select(seederFactory));

            return allSeeders.Distinct().ToArray();
        }
    }
}
