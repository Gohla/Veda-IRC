using System;

namespace Veda.Interface
{
    public interface IPermission
    {
        IPermission DefaultAllowed(IBotGroup group, bool allowed);
        IPermission DefaultTimedLimit(IBotGroup group, ushort limit, TimeSpan timeSpan);

        bool Check(IBotUser user, bool defaultAllowed = true);
        void CheckThrows(IBotUser user, bool defaultAllowed = true);
    }
}
