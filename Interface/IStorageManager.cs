using System;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public enum StorageScope
    {
        Global,
        Server,
        Channel
    }

    public interface IStorageManager : IDisposable
    {
        IStorage Global();
        IStorage Server(IClientConnection connection);
        IStorage Channel(IChannel channel);
    }
}
