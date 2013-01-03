using System;

namespace Veda.Interface
{
    public interface IBotGroup : IComparable<IBotGroup>
    {
        String Name { get; }
        int PrivilegeLevel { get; }

        bool IsMorePrivileged(IBotGroup other);
        bool IsMoreOrSamePrivileged(IBotGroup other);
    }
}
