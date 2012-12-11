using System;
using System.IO;
using Gohla.Shared.Composition;
using ReactiveIRC.Interface;
using Veda.Interface;
using System.Collections.Generic;

namespace Veda.Storage
{
    public class StorageManager : IStorageManager
    {
        private readonly String _path;
        private readonly String _extension;
        private IStorage _global;
        private Dictionary<IClientConnection, IStorage> _server = new Dictionary<IClientConnection, IStorage>();
        private Dictionary<IChannel, IStorage> _channel = new Dictionary<IChannel, IStorage>();

        public StorageManager(String path, String extension, String globalFile)
        {
            _path = path;
            _extension = extension.StartsWith(".") ? extension : "." + extension;
            _global = OpenGlobal(globalFile);
        }

        public void Dispose()
        {
            if(_channel == null)
                return;

            _channel.Do(x => x.Value.Dispose());
            _channel.Clear();
            _channel = null;

            _server.Do(x => x.Value.Dispose());
            _server.Clear();
            _server = null;

            _global.Dispose();
            _global = null;
        }

        public IStorage Global()
        {
            return _global;
        }

        public IStorage Server(IClientConnection connection)
        {
            return _server.GetOrCreate(connection, () => OpenServer(connection));
        }

        public IStorage Channel(IChannel channel)
        {
            return _channel.GetOrCreate(channel, () => OpenChannel(channel));
        }

        private IStorage OpenGlobal(String file)
        {
            IStorage storage = CompositionManager.Get<IStorage>();
            storage.Open(Path.Combine(_path, file + _extension));
            return storage;
        }

        private String ServerString(IClientConnection connection)
        {
            return connection.Address + "_" + connection.Port;
        }

        private IStorage OpenServer(IClientConnection connection)
        {
            IStorage storage = CompositionManager.Get<IStorage>();
            storage.Open(Path.Combine(_path, ServerString(connection) + _extension));
            return storage;
        }

        private String ChannelString(IChannel channel)
        {
            return channel.Name;
        }

        private IStorage OpenChannel(IChannel channel)
        {
            IStorage storage = CompositionManager.Get<IStorage>();
            storage.Open(Path.Combine(_path, ServerString(channel.Connection), ChannelString(channel) + _extension));
            return storage;
        }
    }
}
