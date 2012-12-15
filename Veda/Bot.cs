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

        public Bot(IClient client, IStorageManager storage, ICommandManager command, IPluginManager plugin)
        {
            _client = client;
            _storage = storage;
            _command = command;
            _plugin = plugin;

            _data = storage.Global().GetOrCreate<BotData>(_storageIdentifier);
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

            _storage.Global().Set(_storageIdentifier, _data);
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
            ConversionContext conversionContext = new ConversionContext { Bot = this, Message = message };

            try
            {
                try
                {
                    // Get callable command
                    Context context = new Context { Bot = this, Message = message };
                    ICallable callable = _command.Call(message.Contents, conversionContext, context);
                    if(callable == null)
                        return;
                    context.Command = callable.Command;

                    // Get storage
                    context.Storage = _storage.PluginStorage(callable.Command.Plugin, message.Connection,
                        message.Receiver as IChannel);
                    
                    // Call command and present results.
                    object result = callable.Call();
                    if(result == null)
                    {
                        Reply(message, "The operation succeeded.");
                        return;
                    }

                    IEnumerable<String> replies = result as IEnumerable<String>;
                    if(replies != null)
                        Reply(message, replies);

                    IObservable<String> observableReply = result as IObservable<String>;
                    if(observableReply != null)
                        observableReply.Subscribe(
                            str => Reply(message, str),
                            e => Reply(message, e)
                        );

                    IObservable<IEnumerable<String>> observableReplies = result as IObservable<IEnumerable<String>>;
                    if(observableReplies != null)
                        observableReplies.Subscribe(
                            strs => Reply(message, strs),
                            e => Reply(message, e)
                        );

                    String reply = result.ToString();
                    if(!String.IsNullOrWhiteSpace(reply))
                        Reply(message, reply);
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

        private void Reply(IReceiveMessage message, String reply)
        {
            ReplyTarget(message).SendMessage(reply);
        }

        private void Reply(IReceiveMessage message, IEnumerable<String> replies)
        {
            ReplyTarget(message).SendMessage(replies.ToString("; "));
        }

        private void Reply(IReceiveMessage message, Exception e)
        {
            Reply(message, "Error: " + e.Message);
            _logger.InfoException("Error executing: \"" + message.Contents + "\".", e);
        }
    }
}
