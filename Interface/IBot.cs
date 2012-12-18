using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IBot : IDisposable
    {
        IEnumerable<IClientConnection> Connections { get; }
        IPluginStorageManager Storage { get; }
        ICommandManager Command { get; }
        IPluginManager Plugin { get; }
        IAuthenticationManager Authentication { get; }

        IClientConnection Connect(String address, ushort port, String nickname, String username, String realname,
            String password);
    }
}