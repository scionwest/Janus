using Bogus;
using Janus.SampleApi.Data;
using Janus.Seeding;
using System;
using System.Collections.Generic;

namespace Janus.SampleApi
{
    public class TaskEntitySeeder : EntitySeeder<TaskEntity>
    {
        protected override bool MapEntities(TaskEntity[] seededEntities, ISeedReader seedReader)
        {
            UserEntity[] users = seedReader.GetDataForEntity<UserEntity>();
            var faker = new Faker();

            foreach(TaskEntity task in seededEntities)
            {
                UserEntity user = faker.PickRandom(users);
                user.Tasks.Add(task);
                task.UserId = user.Id;
            }

            return true;
        }

        protected override IList<TaskEntity> Seed(SeedOptions options)
        {
            IList<TaskEntity> tasks = new Faker().Make(5, count =>
            {
                TaskEntity task = new Faker<TaskEntity>("en")
                    .RuleFor(entity => entity.DueDate, faker => faker.Date.Soon())
                    .RuleFor(entity => entity.Id, Guid.NewGuid())
                    .RuleFor(entity => entity.Title, faker => faker.Rant.Review())
                    .Ignore(entity => entity.UserId);

                return task;
            });

            return tasks;
        }
    }
}
