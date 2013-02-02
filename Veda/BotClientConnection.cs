using System;
using System.Reactive.Linq;
using ReactiveIRC.Interface;
using Veda.Configuration;
using Veda.Interface;

namespace Veda
{
    public class BotClientConnection : IDisposable
    {
        private MessageHandler _messageHandler;
        private ConnectionData _data;
        private IDisposable _messageSubscription;

        public IBot Bot { get; private set; }
        public IClientConnection Connection { get; private set; }
        
        public IObservable<IReceiveMessage> ReceivedMessages { get { return Connection.ReceivedMessages; } }

        public BotClientConnection(IBot bot, IClientConnection connection, MessageHandler messageHandler, 
            ConnectionData data)
        {
            Bot = bot;
            Connection = connection;
            _messageHandler = messageHandler;
            _data = data;

            // Subscribe to received messages.
            _messageSubscription = Connection.ReceivedMessages
                .Where(m => !m.Sender.Equals(connection.Me))
                .Where(m => m.Type == ReceiveType.Message || m.Type == ReceiveType.Notice ||
                    m.Type == ReceiveType.Action)
                .Where(m => m.Sender.Type == MessageTargetType.User || m.Sender.Type == MessageTargetType.ChannelUser)
                .Subscribe(_messageHandler.ReceivedMessage);
        }

        public void Dispose()
        {
            if(_messageSubscription == null)
                return;

            _messageSubscription.Dispose();
            _messageSubscription = null;
            Connection.Dispose();
            Connection = null;
        }
    }
}
