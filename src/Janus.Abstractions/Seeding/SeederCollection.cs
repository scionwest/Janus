using System;
using System.Collections.Generic;

namespace Janus.Seeding
{
    public class SeederCollection : ISeederCollection
    {
        private List<Type> seeders = new List<Type>();

        public void AddSeeder<TSeeder>() where TSeeder : IEntitySeeder
        {
            this.seeders.Add(typeof(TSeeder));
        }

        public Type[] GetSeederTypes()
        {
            return this.seeders.ToArray();
        }
    }
}
