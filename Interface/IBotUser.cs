using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IBotUser : IComparable<IBotUser>
    {
        String Username { get; }
        String HashedPassword { get; }
        IBotGroup Group { get; set; }

        bool CheckPassword(String password);
    }
}
