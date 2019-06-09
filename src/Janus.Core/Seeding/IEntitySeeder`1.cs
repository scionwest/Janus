namespace Janus.Seeding
{

    public interface IEntitySeeder<TEntity> : IEntitySeeder
    {
        TEntity[] GetSeedEntities();
    }
}
