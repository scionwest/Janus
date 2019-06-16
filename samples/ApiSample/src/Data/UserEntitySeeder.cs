using Bogus;
using Janus.SampleApi.Data;
using Janus.Seeding;
using System;
using System.Collections.Generic;

namespace Janus.SampleApi
{
    public class User2EntitySeeder : IEntitySeeder<UserEntity>
    {
        public Type SeedType => throw new NotImplementedException();

        public bool BuildRelationships(IDatabaseSeedReader seedRead)
        {
            throw new NotImplementedException();
        }

        public void Generate()
        {
            throw new NotImplementedException();
        }

        public object[] GetSeedData()
        {
            throw new NotImplementedException();
        }

        public UserEntity[] GetSeedEntities()
        {
            throw new NotImplementedException();
        }

        public void ValidateSeedData(IDatabaseSeedReader seedRead)
        {
            throw new NotImplementedException();
        }
    }

    //public class UserEntitySeeder : EntitySeeder<UserEntity>
    //{
    //    protected override bool MapEntities(UserEntity[] seededEntities, ISeedReader seedReader)
    //    {
    //        return true;
    //    }

    //    protected override IList<UserEntity> Seed(SeedOptions options)
    //    {
    //        IList<UserEntity> users = new Faker().Make(3, count =>
    //        {
    //            UserEntity user = new Faker<UserEntity>()
    //            .RuleFor(entity => entity.Address, faker => faker.Address.FullAddress())
    //            .RuleFor(entity => entity.Email, faker => faker.Internet.Email())
    //            .RuleFor(entity => entity.Id, Guid.NewGuid())
    //            .RuleFor(entity => entity.Username, faker => faker.Internet.UserName())
    //            .Ignore(entity => entity.Tasks);

    //            return user;
    //        });

    //        return users;
    //    }
    //}
}
