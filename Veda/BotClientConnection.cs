using System;
using ReactiveIRC.Interface;
using Veda.Configuration;
using Veda.Interface;

namespace Veda
{
    public class BotClientConnection : IDisposable
    {
        private ConnectionData _data;

        public IBot Bot { get; private set; }
        public IClientConnection Connection { get; private set; }
        
        public IObservable<IReceiveMessage> ReceivedMessages { get { return Connection.ReceivedMessages; } }

        public BotClientConnection(IBot bot, IClientConnection connection, ConnectionData data)
        {
            Bot = bot;
            Connection = connection;
            _data = data;
        }

        public void Dispose()
        {
            if(Connection == null)
                return;

            Connection.Dispose();
            Connection = null;
        }
    }
}
