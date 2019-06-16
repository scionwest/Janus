namespace Janus.Seeding
{
    public interface IDatabaseSeedReader
    {
        TEntity[] GetSeededEntities<TEntity>(IEntitySeeder[] entitySeeders);
    }
}
