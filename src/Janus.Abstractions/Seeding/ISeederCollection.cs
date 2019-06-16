using System;

namespace Janus.Seeding
{
    public interface ISeederCollection
    {
        void AddSeeder<TSeeder>() where TSeeder : IEntitySeeder;
        Type[] GetSeederTypes();
    }
}
