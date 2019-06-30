namespace Janus.Seeding
{

    public interface ISeedManager
    {
        ISeedResult BuildSeedData();
        ISeedManager UseSeeder<TSeeder>() where TSeeder : IEntitySeeder, new();
        ISeedManager UseSeeder(params IEntitySeeder[] entitySeeders);
    }
}
