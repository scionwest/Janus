using System;

namespace Janus.Seeding
{
    public interface IEntitySeeder
    {
        Type SeedType { get; }
        void Generate();
        object[] GetSeedData();
        bool BuildRelationships(ISeedReader seedRead);
        void ValidateSeedData(ISeedReader seedRead);
    }
}
