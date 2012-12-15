using System;
using System.Collections.Generic;
using System.IO;
using Gohla.Shared.Composition;
using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda.Storage
{
    public class StorageManager : IStorageManager
    {
        private readonly String _path;
        private readonly String _extension;
        private IStorage _global;
        private Dictionary<IClientConnection, IStorage> _server = new Dictionary<IClientConnection, IStorage>();
        private Dictionary<IChannel, IStorage> _channel = new Dictionary<IChannel, IStorage>();
        private Dictionary<IPlugin, IStorage> _pluginGlobal = new Dictionary<IPlugin, IStorage>();
        private Dictionary<Tuple<IPlugin, IClientConnection>, IStorage> _pluginServer =
            new Dictionary<Tuple<IPlugin, IClientConnection>, IStorage>();
        private Dictionary<Tuple<IPlugin, IChannel>, IStorage> _pluginChannel =
            new Dictionary<Tuple<IPlugin, IChannel>, IStorage>();

        public StorageManager(String path, String extension, String globalFile)
        {
            _path = path;
            _extension = extension.StartsWith(".") ? extension : "." + extension;
            _global = OpenGlobal(globalFile);
        }

        public void Dispose()
        {
            if(_pluginChannel == null)
                return;

            _pluginChannel.Do(x => x.Value.Dispose());
            _pluginChannel.Clear();
            _pluginChannel = null;

            _pluginServer.Do(x => x.Value.Dispose());
            _pluginServer.Clear();
            _pluginServer = null;

            _pluginGlobal.Do(x => x.Value.Dispose());
            _pluginGlobal.Clear();
            _pluginGlobal = null;

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

        public IStorage Global(IPlugin plugin)
        {
            return _pluginGlobal.GetOrCreate(plugin, () => OpenGlobal(plugin));
        }

        public IStorage Server(IPlugin plugin, IClientConnection connection)
        {
            return _pluginServer.GetOrCreate(Tuple.Create(plugin, connection), () => OpenServer(plugin, connection));
        }

        public IStorage Channel(IPlugin plugin, IChannel channel)
        {
            return _pluginChannel.GetOrCreate(Tuple.Create(plugin, channel), () => OpenChannel(plugin, channel));
        }

        public IStorage PluginStorage(IPlugin plugin, IClientConnection connection = null, IChannel channel = null)
        {
            IStorage storage = new NestedStorage(Global(plugin), null);
            if(connection != null)
            {
                storage = new NestedStorage(Server(plugin, connection), storage);
            }
            if(channel != null)
            {
                storage = new NestedStorage(Channel(plugin, channel), storage);
            }
            return storage;
        }

        private String PluginString(IPlugin plugin)
        {
            return plugin.Name;
        }

        private IStorage OpenGlobal(String file)
        {
            IOpenableStorage storage = NewStorage();
            storage.Open(Path.Combine(_path, file + _extension));
            return storage;
        }

        private IStorage OpenGlobal(IPlugin plugin)
        {
            IOpenableStorage storage = NewStorage();
            storage.Open(Path.Combine(_path, PluginString(plugin) + _extension));
            return storage;
        }

        private String ServerString(IClientConnection connection)
        {
            return connection.Address + "_" + connection.Port;
        }

        private IStorage OpenServer(IClientConnection connection)
        {
            IOpenableStorage storage = NewStorage();
            storage.Open(Path.Combine(_path, ServerString(connection) + _extension));
            return storage;
        }

        private IStorage OpenServer(IPlugin plugin, IClientConnection connection)
        {
            IOpenableStorage storage = NewStorage();
            storage.Open(Path.Combine(_path, ServerString(connection), PluginString(plugin) + _extension));
            return storage;
        }

        private String ChannelString(IChannel channel)
        {
            return channel.Name;
        }

        private IStorage OpenChannel(IChannel channel)
        {
            IOpenableStorage storage = NewStorage();
            storage.Open(Path.Combine(_path, ServerString(channel.Connection), ChannelString(channel) + _extension));
            return storage;
        }

        private IStorage OpenChannel(IPlugin plugin, IChannel channel)
        {
            IOpenableStorage storage = NewStorage();
            storage.Open(Path.Combine(_path, ServerString(channel.Connection), ChannelString(channel), 
                PluginString(plugin) + _extension));
            return storage;
        }

        private IOpenableStorage NewStorage()
        {
            return new CachedStorage(CompositionManager.Get<IOpenableStorage>());
        }
    }
}
