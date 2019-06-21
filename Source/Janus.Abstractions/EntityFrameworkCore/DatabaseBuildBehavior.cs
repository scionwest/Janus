using System;

namespace Janus.EntityFrameworkCore
{
    [Flags]
    public enum DatabaseBuildBehavior
    {
        GenerateOnSeed = 1,
        ResetOnSeed = 2,
        DeleteOnShutdown = 4,
        UniqueDatabasePerSeed = 8,
        AlwaysRetain = 16,
    }
}
