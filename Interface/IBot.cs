using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IBot : IDisposable
    {
        IEnumerable<IClientConnection> Connections { get; }
        IStorageManager StorageManager { get; }
        ICommandManager CommandManager { get; }
        IPluginManager PluginManager { get; }

        IClientConnection Connect(String address, ushort port, String nickname, String username, String realname,
            String password);
    }
}