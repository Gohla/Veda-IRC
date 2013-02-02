using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NLog;
using ReactiveIRC.Interface;
using Veda.Authentication;
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
        private IPermissionManager _permission;

        private BotData _data;
        private List<BotClientConnection> _botConnections = new List<BotClientConnection>();

        private ReplyHandler _replyHandler;
        private MessageHandler _messsageHandler;

        public IEnumerable<IClientConnection> Connections { get { return _botConnections.Select(x => x.Connection); } }
        public IObservable<Tuple<IContext, IReceiveMessage>> Messages { get { return _messsageHandler.Messages; } }

        public IPluginStorageManager Storage { get { return _storage; } }
        public ICommandManager Command { get { return _command; } }
        public IPluginManager Plugin { get { return _plugin; } }
        public IAuthenticationManager Authentication { get { return _authentication; } }
        public IPluginPermissionManager Permission { get { return _permission; } }

        public ReplyForm DefaultReplyForm
        {
            get
            {
                return _data.ReplyWithNickname ? ReplyForm.Reply : ReplyForm.Echo;
            }
        }

        public Bot(IClient client, IStorageManager storage, ICommandManager command, IPluginManager plugin,
            IAuthenticationManager authentication, IPermissionManager permission)
        {
            _client = client;
            _storage = storage;
            _command = command;
            _plugin = plugin;
            _authentication = authentication;
            _permission = permission;

            _data = storage.Global().GetOrCreate<BotData>(_storageIdentifier);

            _replyHandler = new ReplyHandler(this, _data);
            _messsageHandler = new MessageHandler(this, _replyHandler, _data);
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
            BotClientConnection botConnection = new BotClientConnection(this, connection, _messsageHandler, data);
            _botConnections.Add(botConnection);
            if(store)
                _data.Connections.Add(data);

            // Create connection
            connection.Connect().Subscribe(
                _ => { },
                e => _logger.ErrorException("Unable to connect.", e),
                () => connection.Login(data.Nickname, data.Username, data.Realname, data.Password).Subscribe()
            );

            return connection;
        }

        public String More()
        {
            return _replyHandler.More();
        }

        public String More(IMessageTarget sender)
        {
            return _replyHandler.More(sender);
        }

        public void Output(IContext context, IReceiveMessage message, IObservable<object> output,
            bool replySuccess = true)
        {
            bool reply = false;
            output
                .ToString(context.Seperator)
                .Subscribe
                (
                    str =>
                    {
                        _replyHandler.Reply(message, context.Sender, context.ReplyForm, str);
                        reply = true;
                    },
                    e =>
                    {
                        _replyHandler.Reply(message, context.Sender, context.ReplyForm, e);
                        reply = true;
                    },
                    () =>
                    {
                        if(!reply && _data.ReplySuccess && replySuccess)
                            _replyHandler.Reply(message, context.Sender, context.ReplyForm, "The operation succeeded.");
                    }
                );
        }
    }
}
