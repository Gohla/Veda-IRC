using System;
using System.Collections.ObjectModel;
using System.Linq;
using Gohla.Shared;
using ReactiveIRC.Interface;
using Veda.Command;
using Veda.Configuration;
using Veda.Storage;

namespace Veda
{
    public class Bot : IBot
    {
        private static readonly String _storageIdentifier = "BotData";

        private IClient _client;
        private IStorageManager _storage;
        private ICommandManager _command;
        private BotData _data;

        public ObservableCollection<IClientConnection> Connections { get; private set; }

        public Bot(IClient client, IStorageManager storage, ICommandManager command)
        {
            _client = client;
            _storage = storage;
            _command = command;

            Connections = new ObservableCollection<IClientConnection>();

            _data = storage.Get<BotData>(_storageIdentifier);
            if(_data == null)
                _data = new BotData();

            foreach(ConnectionData data in _data.Connections.ToArray())
            {
                Connect(data);
            }
        }

        public void Dispose()
        {
            _storage.Set(_storageIdentifier, _data);
        }

        public IClientConnection Connect(ConnectionData data)
        {
            if(!Connections
                .Where(x => x.Address.Equals(data.Address))
                .Where(x => x.Port.Equals(data.Port))
                .IsEmpty())
                return null;

            IClientConnection connection = _client.CreateClientConnection(data.Address, data.Port, null);
            Connections.Add(connection);
            _data.Connections.Add(data);
            connection.Connect().Subscribe(
                _ => { },
                e => Console.WriteLine(e.Message),
                () => connection.Login(data.Nickname, data.Username, data.Realname, data.Password).Subscribe()
            );
            connection.ReceivedMessages.Subscribe(x => Console.WriteLine(x.Contents));
            return connection;
        }
    }
}
