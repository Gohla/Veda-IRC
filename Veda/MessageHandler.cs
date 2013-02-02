using System;
using System.Reactive.Subjects;
using NLog;
using ReactiveIRC.Interface;
using Veda.Configuration;
using Veda.Interface;

namespace Veda
{
    public class MessageHandler
    {
        private static readonly Logger _logger = LogManager.GetLogger("MessageHandler");

        private Bot _bot;
        private ReplyHandler _replyHandler;
        private BotData _data;

        private ISubject<Tuple<IContext, IReceiveMessage>> _messages = new Subject<Tuple<IContext, IReceiveMessage>>();

        public IObservable<Tuple<IContext, IReceiveMessage>> Messages { get { return _messages; } }

        public MessageHandler(Bot bot, ReplyHandler replyHandler, BotData data)
        {
            _bot = bot;
            _replyHandler = replyHandler;
            _data = data;
        }

        public void ReceivedMessage(IReceiveMessage message)
        {
            try
            {
                bool privateMessage = message.Receiver.Equals(message.Connection.Me);
                String contents = ProcessPrefix(message, privateMessage);
                ConversionContext conversionContext = new ConversionContext { Bot = _bot, Message = message };
                IUser sender = message.Sender as IUser ?? (message.Sender as IChannelUser).User;
                IBotUser botUser = _bot.Authentication.GetUser(sender);
                IChannel channel = message.Receiver as IChannel;

                Context context = new Context
                {
                    Bot = _bot

                    , Connection = message.Connection
                    , Sender = sender
                    , Channel = channel
                    , User = botUser
                    , Contents = contents ?? message.Contents

                    , Allowed = command => Allowed(command, privateMessage, botUser)
                    , ConversionContext = conversionContext
                    , CallDepth = 0
                    , ReplyForm = privateMessage ? ReplyForm.Echo : _bot.DefaultReplyForm
                    , Seperator = "; "
                };

                if(contents == null)
                {
                    _messages.OnNext(Tuple.Create<IContext, IReceiveMessage>(context, message));
                    return;
                }

                try
                {
                    ICallable callable = _bot.Command.Call(contents);
                    if(callable == null)
                        return;

                    _bot.Output(context, message, context.Evaluate(callable), true);
                }
                catch(NoCommandNameException e)
                {
                    if(_data.ReplyNoCommand)
                        _replyHandler.Reply(message, sender, _bot.DefaultReplyForm, e);
                }
                catch(NoCommandException e)
                {
                    if(_data.ReplyNoCommand)
                        _replyHandler.Reply(message, sender, _bot.DefaultReplyForm, e);
                }
                catch(AmbiguousCommandsException e)
                {
                    if(_data.ReplyAmbiguousCommands)
                        _replyHandler.Reply(message, sender, _bot.DefaultReplyForm, e);
                }
                catch(IncorrectArgumentsException e)
                {
                    if(_data.ReplyIncorrectArguments)
                        _replyHandler.Reply(message, sender, _bot.DefaultReplyForm, e);
                }
                catch(Exception e)
                {
                    _replyHandler.Reply(message, sender, _bot.DefaultReplyForm, e);
                }
            }
            catch(Exception e)
            {
                _logger.ErrorException("Error executing: \"" + message.Contents + "\".", e);
            }
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


        private void Allowed(ICommand command, bool privateMessage, IBotUser botUser)
        {
            if(command.Private && !privateMessage)
                throw new InvalidOperationException("This command can only be sent in a private message.");

            IPermission permission = _bot.Permission.GetPermission(command, botUser.Group);
            if(command.DefaultPermissions.Length > 0)
            {
                permission.CheckThrows(botUser, false);
            }
            else
            {
                permission.CheckThrows(botUser, true);
            }
        }
    }
}
