namespace Janus.Seeding
{
    public interface ISeederFactory
    {
        IEntitySeeder[] CreateSeeders();
    }
}
