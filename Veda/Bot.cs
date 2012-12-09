using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gohla.Shared;
using NLog;
using ReactiveIRC.Interface;
using Veda.Configuration;
using Veda.Interface;

namespace Veda
{
    public class Bot : IBot
    {
        private static readonly String _storageIdentifier = "BotData";

        private readonly Logger _logger = LogManager.GetLogger("Bot");

        private IClient _client;
        private IStorageManager _storage;
        private ICommandManager _command;
        private IPluginManager _plugin;

        private BotData _data;
        private List<BotClientConnection> _botConnections = new List<BotClientConnection>();
        private ISubject<IReceiveMessage> _receivedMessages = new Subject<IReceiveMessage>();

        public IEnumerable<IClientConnection> Connections
        {
            get
            {
                return _botConnections.Select(x => x.Connection);
            }
        }
        public ICommandManager CommandManager { get { return _command; } }
        public IPluginManager PluginManager { get { return _plugin; } }

        public Bot(IClient client, IStorageManager storage, ICommandManager command, IPluginManager plugin)
        {
            _client = client;
            _storage = storage;
            _command = command;
            _plugin = plugin;

            _data = storage.Get<BotData>(_storageIdentifier);
            if(_data == null)
                _data = new BotData();

            _receivedMessages.Subscribe(ReceivedMessage);

            foreach(ConnectionData data in _data.Connections.ToArray())
            {
                Connect(data);
            }
        }

        public void Dispose()
        {
            if(_botConnections == null)
                return;

            _botConnections.ForEach(x => x.Dispose());
            _botConnections.Clear();
            _botConnections = null;

            _storage.Set(_storageIdentifier, _data);
        }

        public IClientConnection Connect(String address, ushort port, String nickname, String username, String realname,
            String password)
        {
            return Connect(new ConnectionData
            {
                Address = address, Port = port, Nickname = nickname, Username = username, Realname = realname, 
                Password = password
            });
        }

        private IClientConnection Connect(ConnectionData data)
        {
            // Prevent duplicate connections
            if(!Connections
                .Where(x => x.Address.Equals(data.Address))
                .Where(x => x.Port.Equals(data.Port))
                .IsEmpty())
                return null;

            // Create and store objects.
            IClientConnection connection = _client.CreateClientConnection(data.Address, data.Port, null);
            BotClientConnection botConnection = new BotClientConnection(this, connection, data);
            _botConnections.Add(botConnection);
            _data.Connections.Add(data);

            // Subscribe to received messages.
            botConnection.ReceivedMessages
                .Where(m => !m.Sender.Equals(connection.Me))
                .Where(m => m.Type == ReceiveType.Message || m.Type == ReceiveType.Notice)
                .Subscribe(_receivedMessages);

            // Create connection
            connection.Connect().Subscribe(
                _ => { },
                e => _logger.ErrorException("Unable to connect.", e),
                () => connection.Login(data.Nickname, data.Username, data.Realname, data.Password).Subscribe()
            );

            return connection;
        }

        private void ReceivedMessage(IReceiveMessage message)
        {
            IClientConnection connection = message.Connection;
            Context context = new Context { Bot = this, Connection = connection, Message = message };

            try
            {
                String result = null;

                try
                {
                    result = _command.Call(message.Contents, this, context)() as String;
                }
                catch(Exception e)
                {
                    result = "Error: " + e.Message;
                    _logger.InfoException("Error executing: \"" + message.Contents + "\".", e);
                }
                
                if(result != null)
                {
                    IMessageTarget replyTarget = null;
                    if(message.Receiver.Equals(connection.Me))
                        replyTarget = message.Sender;
                    else
                        replyTarget = message.Receiver;
                    replyTarget.SendMessage(result);
                }
            }
            catch(Exception e)
            {
                _logger.ErrorException("Error executing: \"" + message.Contents + "\".", e);
            }
        }
    }
}
