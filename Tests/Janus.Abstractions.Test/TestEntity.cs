using System;
using System.Collections.Generic;

namespace Janus.Seeding
{
    public class TestEntity
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
    }
    public class FooEntity
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
    }
    public class BarEntity
    {
        public Guid Id { get; set; }
        public long Epoch { get; set; }
        public FooEntity Foo { get; set; }
    }

    public class FooEntitySeeder : JanusEntitySeeder<FooEntity>
    {
        protected override bool MapEntities(FooEntity[] seededEntities, ISeedReader seedReader)
        {
            // Nothing to Map for FooEntity
            return true;
        }

        protected override IList<FooEntity> Seed(JanusSeedOptions options)
        {
            return new List<FooEntity>
            {
                new FooEntity { Value = "Hello" },
                new FooEntity { Value = "World" },
            };
        }
    }

    public class BarEntitySeeder : JanusEntitySeeder<BarEntity>
    {
        protected override bool MapEntities(BarEntity[] seededEntities, ISeedReader seedReader)
        {
            FooEntity[] foos = seedReader.GetSeededEntities<FooEntity>();
            if (foos.Length != 2 || seededEntities.Length != 2)
            {
                return false;
            }

            seededEntities[0].Foo = foos[0];
            seededEntities[1].Foo = foos[1];
            return true;
        }

        protected override IList<BarEntity> Seed(JanusSeedOptions options)
        {
            return new List<BarEntity>
            {
                new BarEntity { Epoch = 1561002833 },
                new BarEntity { Epoch = 1561002855 },
            };
        }
    }
}
