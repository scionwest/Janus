using System;

namespace Janus.EntityFrameworkCore
{
    [Flags]
    public enum DatabaseBuildBehavior
    {
        UniquePerBuild = 1,
        ResetOnBuild = 2,
        DeleteOnShutdown = 4,
        AlwaysRetain = 8,
    }
}
