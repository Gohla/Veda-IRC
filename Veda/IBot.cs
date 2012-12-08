using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;
using Veda.Command;
using Veda.Configuration;
using Veda.Plugin;

namespace Veda
{
    public interface IBot : IDisposable
    {
        IEnumerable<IClientConnection> Connections { get; }
        ICommandManager CommandManager { get; }
        IPluginManager PluginManager { get; }

        IClientConnection Connect(ConnectionData data);
    }
}
