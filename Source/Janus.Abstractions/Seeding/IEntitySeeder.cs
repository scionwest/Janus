using System;

namespace Janus.Seeding
{
    public interface IEntitySeeder
    {
        Type SeedType { get; }
        void Generate();
        object[] GetSeedData();
        bool BuildRelationships(ISeedReader seedReader);
        void ValidateSeedData(ISeedReader seedReader);
    }
}
