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
        IBotUser Authenticate(IUser user, String username, String password);

        IBotUser GetUser(IUser user);
    }
}
