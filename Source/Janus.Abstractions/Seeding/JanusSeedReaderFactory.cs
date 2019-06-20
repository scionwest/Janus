namespace Janus.Seeding
{
    public class JanusSeedReaderFactory : ISeedReaderFactory
    {
        public ISeedReader CreateReader(IEntitySeeder[] seeders)
        {
            return new JanusSeedReader(seeders);
        }
    }
}
