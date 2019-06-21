using System;
using System.Collections.Generic;

namespace Janus.Seeding
{
    public interface ISeedResult
    {
        TEntity[] GetSeedDataForEntity<TEntity>();
        Dictionary<Type, object[]> GetSeedData();
    }
}
