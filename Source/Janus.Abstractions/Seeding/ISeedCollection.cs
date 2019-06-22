using System;

namespace Janus.Seeding
{
    public interface ISeedCollection
    {
        Type[] GetSeederTypes();
    }
}
