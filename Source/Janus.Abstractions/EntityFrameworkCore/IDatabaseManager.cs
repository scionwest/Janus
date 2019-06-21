using System;

namespace Janus.EntityFrameworkCore
{
    public interface IDatabaseManager
    {
        JanusDatabaseManagerOptions DefaultOptions { get; }
    }
}
