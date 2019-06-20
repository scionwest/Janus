# Entity Seeding

Entity Seeding can be done by either implementing the `IEntitySeeder<TEntity>` interface, or by inheriting the `JanusEntitySeeder<TEntity>` abstract class.
The abstract class makes implementing a seeder the easiest as most of the heavy lifting is done for you.

Creating a sub-class of `JanusEntitySeeder<TEntity>` is easy. Let's start with an entity that we want to seed with data.

```c#
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
```

You can then subclass the `JanusEntitySeeder<TEntity>` and implement the `MapEntities` and `Seed` methods for a seeder that seeds `FooEntity` data.

```c#
    public class FooEntitySeeder : JanusEntitySeeder<FooEntity>
    {
        protected override bool MapEntities(FooEntity[] seededEntities, IDatabaseSeedReader seedReader)
        {
            // Nothing to Map for FooEntity
            return true;
        }

        protected override IList<FooEntity> Seed(JanusDatabaseSeedOptions options)
        {
            return new List<FooEntity>
            {
                new FooEntity { Value = "Hello" },
                new FooEntity { Value = "World" },
            };
        }
    }
```
The `MapEntities` method returns true because we don't need to map anything to the `FooEntity` instances. Our `Seed` method is used to actually create our fake data. 
Next, we can seed `FooEntity` with another seeder.

```c#
    public class BarEntitySeeder : JanusEntitySeeder<BarEntity>
    {
        protected override bool MapEntities(BarEntity[] seededEntities, IDatabaseSeedReader seedReader)
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

        protected override IList<BarEntity> Seed(JanusDatabaseSeedOptions options)
        {
            return new List<BarEntity>
            {
                new BarEntity { Epoch = 1561002833 },
                new BarEntity { Epoch = 1561002855 },
            };
        }
    }
```

This time, we implement the `MapEntities` method so that we can pull in previously seeded `FooEntities` and associate them with each instance of `BarEntity`. This is how we build the relationships out.
If you make a call to `IDatabaseSeedReader` to get a set of previously seeded entities, and nothing is returned to you then you can return `false`. When you return `false` then seeding can continue with the remaining seeders. When all seeders are completed with their relationships, the seeders that returned false are given another opportunity to seed the data. This cycle continues until all seeders either return true, or all of them return false.

You can make your seed data a lot better by using the [Bogus framework](https://github.com/bchavez/Bogus).

```c#
    public class BarEntitySeeder : JanusEntitySeeder<BarEntity>
    {
        protected override bool MapEntities(BarEntity[] seededEntities, IDatabaseSeedReader seedReader)
        {
			var faker = new Faker();
            FooEntity[] foos = seedReader.GetSeededEntities<FooEntity>();

			if (foos.Lenght == 0) 
				return false;

			foreach(BarEntity bar in seededEntities)
			{
				FooEntity foo = faker.PickRandom(foos);
				bar.Foo = foo;
			}

			return true;
        }

        protected override IList<BarEntity> Seed(JanusDatabaseSeedOptions options)
        {
			IList<BarEntity> entities = new Faker().Make(100, index =>
			{
				BarEntity entity - new Faker<BarEntity>()
					.RuleFor(entity => entity.Epoch, faker => faker.Random.Long())
					.Ignore(entity => entity.Id)
					.Ignore(entity => entity.Foo);

					return entity;
			});
        }
    }
```

This will generate 100 instances of `BarEntity` and then loop through each instance and assign an instance of `FooEntity` to it. All of the data is generated and the `FooEntity` instance is picked randomly for each `BarEntity` instance.

You can also validate each entity if you want by overriding the `IsEntityValid` method. This will let you check each entity and make sure the data is valid. If the data generated was invalid, you can return false. Returning false will have the instance removed from the generated data list.

```c#
    public class BarEntitySeeder : JanusEntitySeeder<BarEntity>
    {
        protected override bool MapEntities(BarEntity[] seededEntities, IDatabaseSeedReader seedReader)
        {
			var faker = new Faker();
            FooEntity[] foos = seedReader.GetSeededEntities<FooEntity>();

			if (foos.Lenght == 0) 
				return false;

			foreach(BarEntity bar in seededEntities)
			{
				FooEntity foo = faker.PickRandom(foos);
				bar.Foo = foo;
			}

			return true;
        }

		protected override bool IsEntityValid(BarEntity bar, IDatabaseSeedReader seedReader)
		{
			return bar.Foo != null;
		}

        protected override IList<BarEntity> Seed(JanusDatabaseSeedOptions options)
        {
			IList<BarEntity> entities = new Faker().Make(100, index =>
			{
				BarEntity entity - new Faker<BarEntity>()
					.RuleFor(entity => entity.Epoch, faker => faker.Random.Long())
					.Ignore(entity => entity.Id)
					.Ignore(entity => entity.Foo);

					return entity;
			});
        }
    }
```