using System;

namespace Janus.Seeding
{
    public interface IEntitySeeder
    {
        Type SeedType { get; }
        void Generate();
        object[] GetSeedData();
        bool BuildRelationships(IDatabaseSeedReader seedRead);
        void ValidateSeedData(IDatabaseSeedReader seedRead);
    }
}
