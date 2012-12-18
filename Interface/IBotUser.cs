using System;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IBotUser
    {
        String Username { get; }
        IUser User { get; }
        IBotGroup Group { get; }

        bool CheckPassword(String password);
    }
}
