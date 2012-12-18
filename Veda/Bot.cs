using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NLog;
using ReactiveIRC.Interface;
using Veda.Configuration;
using Veda.Interface;
using Veda.Storage;

namespace Veda
{
    public class Bot : IBot
    {
        private static readonly Logger _logger = LogManager.GetLogger("Bot");

        private static readonly String _storageIdentifier = "BotData";

        private IClient _client;
        private IStorageManager _storage;
        private ICommandManager _command;
        private IPluginManager _plugin;
        private IAuthenticationManager _authentication;

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
        public IPluginStorageManager Storage { get { return _storage; } }
        public ICommandManager Command { get { return _command; } }
        public IPluginManager Plugin { get { return _plugin; } }
        public IAuthenticationManager Authentication { get { return _authentication; } }

        public Bot(IClient client, IStorageManager storage, ICommandManager command, IPluginManager plugin,
            IAuthenticationManager authentication)
        {
            _client = client;
            _storage = storage;
            _command = command;
            _plugin = plugin;
            _authentication = authentication;

            _data = storage.Global().GetOrCreate<BotData>(_storageIdentifier);

            _receivedMessages.Subscribe(ReceivedMessage);
        }

        public void Init()
        {
            foreach(ConnectionData data in _data.Connections.ToArray())
            {
                Connect(data, false);
            }
        }

        public void Dispose()
        {
            if(_botConnections == null)
                return;

            _botConnections.ForEach(x => x.Dispose());
            _botConnections.Clear();
            _botConnections = null;
        }

        public IClientConnection Connect(String address, ushort port, String nickname, String username, String realname,
            String password)
        {
            return Connect(new ConnectionData
            {
                Address = address, Port = port, Nickname = nickname, Username = username, Realname = realname,
                Password = password
            }, true);
        }

        private IClientConnection Connect(ConnectionData data, bool store)
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
            if(store)
                _data.Connections.Add(data);

            // Subscribe to received messages.
            botConnection.ReceivedMessages
                .Where(m => !m.Sender.Equals(connection.Me))
                .Where(m => m.Type == ReceiveType.Message || m.Type == ReceiveType.Notice ||
                    m.Type == ReceiveType.Action)
                .Where(m => m.Sender.Type == MessageTargetType.User)
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
            ConversionContext conversionContext = new ConversionContext { Bot = this, Message = message };

            try
            {
                try
                {
                    ICallable callable = _command.Call(message.Contents, conversionContext);
                    if(callable == null)
                        return;

                    IBotUser botUser = _authentication.GetUser(message.Sender as IUser);
                    IStorage storage = _storage.PluginStorage(callable.Command.Plugin, message.Connection,
                        message.Receiver as IChannel);
                    Context context = new Context
                    {
                        Bot = this, Message = message, Storage = storage, Sender = botUser,
                        Command = callable.Command, ConversionContext = conversionContext, CallDepth = 0
                    };

                    bool reply = false;
                    context.Evaluate(callable).Subscribe(
                        str => { Reply(message, str); reply = true; },
                        e => { Reply(message, e); reply = true; },
                        () => { if(!reply) Reply(message, "The operation succeeded."); }
                    );
                }
                catch(Exception e)
                {
                    Reply(message, e);
                }
            }
            catch(Exception e)
            {
                _logger.ErrorException("Error executing: \"" + message.Contents + "\".", e);
            }
        }

        private IMessageTarget ReplyTarget(IReceiveMessage message)
        {
            if(message.Receiver.Equals(message.Connection.Me))
                return message.Sender;
            else
                return message.Receiver;
        }

        private void Reply(IReceiveMessage message, object reply)
        {
            ReplyTarget(message).SendMessage(reply.ToString());
        }

        private void Reply(IReceiveMessage message, Exception e)
        {
            Reply(message, "Error: " + e.Message);
            _logger.InfoException("Error executing: \"" + message.Contents + "\".", e);
        }
    }
}
