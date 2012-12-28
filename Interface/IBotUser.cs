using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IBotUser
    {
        String Username { get; }
        IBotGroup Group { get; }
        IEnumerable<IdentityMask> Masks { get; }

        bool CheckPassword(String password);

        bool AddMask(IdentityMask mask);
        bool RemoveMask(IdentityMask mask);
    }
}
