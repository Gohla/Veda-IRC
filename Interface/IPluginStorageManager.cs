using System;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IPluginStorageManager : IDisposable
    {
        IStorage Global(IPlugin plugin);
        IStorage Server(IPlugin plugin, IClientConnection connection);
        IStorage Channel(IPlugin plugin, IChannel channel);
        IStorage PluginStorage(IPlugin plugin, IClientConnection connection = null, IChannel channel = null);
    }
}
