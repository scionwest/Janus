using System;

namespace Janus
{
    [Flags]
    public enum DatabaseBehavior
    {
        GenerateOnSeed = 1,
        ResetOnSeed = 2,
        DeleteOnShutdown = 4,
        UniqueDatabasePerSeed = 8,
        AlwaysRetain = 16,
    }
}
