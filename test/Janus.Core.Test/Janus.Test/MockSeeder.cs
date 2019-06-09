using Janus.Seeding;
using System.Collections.Generic;

namespace Janus.Test
{
    public class MockSeeder : EntitySeeder<TestEntity>
    {
        protected override bool MapEntities(TestEntity[] seededEntities, ISeedReader seedReader)
        {
            return true;
        }

        protected override IList<TestEntity> Seed(SeedOptions options)
        {
            return new List<TestEntity>();
        }
    }
}
