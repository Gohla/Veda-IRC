using System;
using System.Collections.ObjectModel;
using ReactiveIRC.Interface;
using Veda.Configuration;
using Veda.Storage;
using System.Linq;
using Gohla.Shared;

namespace Veda
{
    public class Bot : IDisposable
    {
        private IClient _client;
        private IStorageManager _storage;
        private BotData _data;

        public ObservableCollection<IClientConnection> Connections { get; private set; }

        public Bot(IClient client, IStorageManager storage)
        {
            _client = client;
            _storage = storage;

            Connections = new ObservableCollection<IClientConnection>();

            _data = storage.Get<BotData>("BotData");
            if(_data == null)
                _data = new BotData();

            foreach(ConnectionData data in _data.Connections)
            {
                Connect(data);
            }
        }

        public void Dispose()
        {
            _storage.Set("BotData", _data);
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
                () => connection.Login(data.Nickname, data.Username, data.Realname, data.Password)
            );
            return connection;
        }
    }
}
