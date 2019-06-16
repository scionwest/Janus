namespace Janus.Seeding
{
    public interface ISeedReader
    {
        TEntity[] GetDataForEntity<TEntity>();
    }
}
