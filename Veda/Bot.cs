using System;
using System.Collections.ObjectModel;
using ReactiveIRC.Interface;

namespace Veda
{
    public class Bot
    {
        private IClient _client;

        public ObservableCollection<IClientConnection> Connections { get; private set; }

        public Bot(IClient client)
        {
            _client = client;

            Connections = new ObservableCollection<IClientConnection>();
        }

        public IClientConnection Connect(String address, ushort port)
        {
            IClientConnection connection = _client.CreateClientConnection(address, port, null);
            Connections.Add(connection);
            connection.Connect().Subscribe(
                _ => { },
                () => connection.Login("Veda2", "Veda2", "Veda2")
            );
            return connection;
        }
    }
}
