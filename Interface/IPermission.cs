using System;

namespace Veda.Interface
{
    public interface IPermission
    {
        String Name { get; }
        IBotGroup Group { get; }

        bool Allowed { get; set; }
        ushort Limit { get; set; }
        TimeSpan Timespan { get; set; }

        void DefaultAllowed(bool allowed);
        void DefaultTimedLimit(ushort limit, TimeSpan timeSpan);

        bool Check(IBotUser user, bool defaultAllowed = true);
        void CheckThrows(IBotUser user, bool defaultAllowed = true);
    }
}
