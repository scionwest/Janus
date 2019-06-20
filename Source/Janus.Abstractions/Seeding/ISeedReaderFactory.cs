namespace Janus.Seeding
{
    public interface ISeedReaderFactory
    {
        ISeedReader CreateReader(IEntitySeeder[] seeders);
    }
}
