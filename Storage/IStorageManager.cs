using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda.Storage
{
    public interface IStorageManager : IPluginStorageManager
    {
        IStorage Global();
        IStorage Server(IClientConnection connection);
        IStorage Channel(IChannel channel);
    }
}
