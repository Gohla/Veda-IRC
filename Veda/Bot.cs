﻿using System;
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
        private ISubject<IReceiveMessage> _receivedMessages = new Subject<IReceiveMessage>();
        private ISubject<Tuple<IContext, IReceiveMessage>> _messages = new Subject<Tuple<IContext, IReceiveMessage>>();

        public IEnumerable<IClientConnection> Connections
        {
            get
            {
                return _botConnections.Select(x => x.Connection);
            }
        }

        public IObservable<Tuple<IContext, IReceiveMessage>> Messages { get { return _messages; } }

        public IPluginStorageManager Storage { get { return _storage; } }
        public ICommandManager Command { get { return _command; } }
        public IPluginManager Plugin { get { return _plugin; } }
        public IAuthenticationManager Authentication { get { return _authentication; } }
        public IPluginPermissionManager Permission { get { return _permission; } }

        private ReplyForm DefaultReplyForm
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
                        Reply(message, context.Sender, context.ReplyForm, str);
                        reply = true;
                    },
                    e =>
                    {
                        Reply(message, context.Sender, context.ReplyForm, e);
                        reply = true;
                    },
                    () =>
                    {
                        if(!reply && _data.ReplySuccess && replySuccess)
                            Reply(message, context.Sender, context.ReplyForm, "The operation succeeded.");
                    }
                );
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
                .Where(m => m.Sender.Type == MessageTargetType.User || m.Sender.Type == MessageTargetType.ChannelUser)
                .Subscribe(_receivedMessages);

            // Create connection
            connection.Connect().Subscribe(
                _ => { },
                e => _logger.ErrorException("Unable to connect.", e),
                () => connection.Login(data.Nickname, data.Username, data.Realname, data.Password).Subscribe()
            );

            return connection;
        }

        private String ProcessPrefix(IReceiveMessage message, bool privateMessage)
        {
            String contents = message.Contents;

            if(privateMessage)
                return contents;

            if(String.IsNullOrWhiteSpace(contents) || contents.Length < 2)
                return null;

            foreach(char prefix in _data.AddressedCharacters)
                if(contents[0] == prefix)
                    return contents.Substring(1);

            if(_data.AddressedNickame)
            {
                String nickname = message.Connection.Me.Name;
                if(contents.Length > nickname.Length + 2 && contents.StartsWith(nickname))
                {
                    contents = contents.Substring(nickname.Length);
                    if(contents[0] == ':' || contents[0] == ',')
                        return contents.Substring(1);
                }
            }

            return null;
        }

        private void ReceivedMessage(IReceiveMessage message)
        {
            try
            {
                bool privateMessage = message.Receiver.Equals(message.Connection.Me);
                String contents = ProcessPrefix(message, privateMessage);
                ConversionContext conversionContext = new ConversionContext { Bot = this, Message = message };
                IUser sender = message.Sender as IUser ?? (message.Sender as IChannelUser).User;
                IBotUser botUser = _authentication.GetUser(sender);
                IChannel channel = message.Receiver as IChannel;

                Context context = new Context
                {
                    Bot = this

                    , Connection = message.Connection
                    , Sender = sender
                    , Channel = channel
                    , User = botUser
                    , Contents = contents ?? message.Contents

                    , Allowed = command => Allowed(command, privateMessage, botUser)
                    , ConversionContext = conversionContext
                    , CallDepth = 0
                    , ReplyForm = privateMessage ? ReplyForm.Echo : DefaultReplyForm
                    , Seperator = "; "
                };

                if(contents == null)
                {
                    _messages.OnNext(Tuple.Create<IContext, IReceiveMessage>(context, message));
                    return;
                }

                try
                {
                    ICallable callable = _command.Call(contents);
                    if(callable == null)
                        return;

                    Output(context, message, context.Evaluate(callable), true);
                }
                catch(NoCommandNameException e)
                {
                    if(_data.ReplyNoCommand)
                        Reply(message, sender, DefaultReplyForm, e);
                }
                catch(NoCommandException e)
                {
                    if(_data.ReplyNoCommand)
                        Reply(message, sender, DefaultReplyForm, e);
                }
                catch(AmbiguousCommandsException e)
                {
                    if(_data.ReplyAmbiguousCommands)
                        Reply(message, sender, DefaultReplyForm, e);
                }
                catch(IncorrectArgumentsException e)
                {
                    if(_data.ReplyIncorrectArguments)
                        Reply(message, sender, DefaultReplyForm, e);
                }
                catch(Exception e)
                {
                    Reply(message, sender, DefaultReplyForm, e);
                }
            }
            catch(Exception e)
            {
                _logger.ErrorException("Error executing: \"" + message.Contents + "\".", e);
            }
        }

        private void Allowed(ICommand command, bool privateMessage, IBotUser botUser)
        {
            if(command.Private && !privateMessage)
                throw new InvalidOperationException("This command can only be sent in a private message.");

            IPermission permission = _permission.GetPermission(command, botUser.Group);
            if(command.DefaultPermissions.Length > 0)
            {
                permission.CheckThrows(botUser, false);
            }
            else
            {
                permission.CheckThrows(botUser, true);
            }
        }

        private IMessageTarget ReplyTarget(IReceiveMessage message)
        {
            if(message.Receiver.Equals(message.Connection.Me))
                return message.Sender;
            else
                return message.Receiver;
        }

        private void Reply(IReceiveMessage message, IMessageTarget sender, ReplyForm replyForm, object result)
        {
            IMessageTarget target = ReplyTarget(message);
            switch(replyForm)
            {
                case ReplyForm.Echo:
                    target.SendMessage(result.ToString());
                    break;
                case ReplyForm.Reply:
                    target.SendMessage(sender.Name + ": " + result.ToString());
                    break;
                case ReplyForm.Action:
                    target.SendAction(result.ToString());
                    break;
                case ReplyForm.Notice:
                    target.SendNotice(result.ToString());
                    break;
            }
            
        }

        private void Reply(IReceiveMessage message, IMessageTarget sender, ReplyForm replyForm, Exception e)
        {
            LogUserError(message, e);

            if(!_data.ReplyError)
                return;
            
            if(_data.ReplyErrorDetailed)
                Reply(message, sender, replyForm, "Error -- " + e.Message);
            else
                Reply(message, sender, replyForm, "An error occurred.");
        }

        private void LogUserError(IReceiveMessage message, Exception e)
        {
            _logger.InfoException("Error executing: \"" + message.Contents + "\".", e);
        }
    }
}
