namespace Janus.Seeding
{
    public interface ISeedReader
    {
        TEntity[] GetSeededEntities<TEntity>();
    }
}
