using System;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IAuthenticationManager : IDisposable
    {
        IBotGroup Guest { get; }
        IBotGroup Registered { get; }
        IBotGroup Administrator { get; }
        IBotGroup Owner { get; }

        IBotUser Register(IUser user, String username, String password);
        IBotUser Identify(IUser user, String username, String password);
        IBotUser GetUser(IUser user);
        IBotGroup GetGroup(String name);

        bool IsIdentified(IUser user);
    }
}
