using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using ReactiveIRC.Interface;
using Veda.Configuration;
using Veda.Interface;

namespace Veda
{
    public class ReplyHandler
    {
        private static readonly Logger _logger = LogManager.GetLogger("ReplyHandler");
        private const ushort MAX_MESSAGE_LENGTH = 400;

        private readonly Bot _bot;
        private readonly BotData _data;
        private readonly int _moreMessagesReservedLength;
        private readonly Dictionary<IMessageTarget, Queue<String>> _moreMessages =
            new Dictionary<IMessageTarget, Queue<String>>();
        private IMessageTarget _lastMore;

        public ReplyHandler(Bot bot, BotData data)
        {
            _bot = bot;
            _data = data;

            _moreMessagesReservedLength = MoreMessages(_data.MaxMores).Length;
        }

        public void Reply(IReceiveMessage message, IMessageTarget sender, ReplyForm replyForm, object result)
        {
            IMessageTarget target = ReplyTarget(message);
            ISendMessage sendMessage = ReplyMessage(message.Connection.MessageSender, target, replyForm, result);

            if(replyForm != ReplyForm.More)
            {
                String[] messages = SplitMessage(sendMessage).ToArray();
                if(messages.Length > _data.ReplyMores)
                {
                    _moreMessages.Remove(sender);
                    _moreMessages.Add(sender, new Queue<String>(messages.Skip(_data.ReplyMores)));
                    _lastMore = sender;
                }

                message.Connection.SendAndForget(messages
                    .Take(_data.ReplyMores)
                    .Select(s => SendMessage(sendMessage, s, sender, replyForm))
                );
            }
            else
            {
                message.Connection.SendAndForget(SendMessage(sendMessage, sendMessage.Contents, sender, 
                    _bot.DefaultReplyForm));
            }
        }

        public void Reply(IReceiveMessage message, IMessageTarget sender, ReplyForm replyForm, Exception e)
        {
            LogUserError(message, e);

            if(!_data.ReplyError)
                return;

            if(_data.ReplyErrorDetailed)
                Reply(message, sender, replyForm, "Error -- " + e.Message);
            else
                Reply(message, sender, replyForm, "An error occurred.");
        }

        public String More()
        {
            if(_lastMore == null)
                throw new InvalidOperationException("No more messages in the buffer.");
            return More(_lastMore);
        }

        public String More(IMessageTarget sender)
        {
            if(!_moreMessages.ContainsKey(sender))
                throw new InvalidOperationException("No more messages in the buffer.");
            Queue<String> moreMessages = _moreMessages[sender];
            if(moreMessages.Count == 0)
                throw new InvalidOperationException("No more messages in the buffer.");

            return moreMessages.Dequeue();
        }

        private ISendMessage SendMessage(ISendMessage message, String contents, IMessageTarget sender, 
            ReplyForm replyForm)
        {
            if(replyForm == ReplyForm.Reply)
                contents = sender.Name + ": " + contents;
            return message.Connection.Client.CreateSendMessage(message, contents);
        }

        private ISendMessage ReplyMessage(IMessageSender messageSender, IMessageTarget target, ReplyForm replyForm, 
            object result)
        {
            switch(replyForm)
            {
                case ReplyForm.Action: return messageSender.Action(target, result.ToString());
                case ReplyForm.Notice: return messageSender.Notice(target, result.ToString());
                default: return messageSender.Message(target, result.ToString());
            }
        }

        private IEnumerable<String> SplitMessage(ISendMessage message)
        {
            int headerLength = message.PrefixHeader.Length + message.PostfixHeader.Length;
            int contentLength = message.Contents.Length;
            int maxContentLength = MAX_MESSAGE_LENGTH - headerLength - _moreMessagesReservedLength;

            if(contentLength > maxContentLength)
            {
                if(!_data.UseMores)
                {
                    yield return message.Contents.Substring(0, maxContentLength);
                }
                else
                {
                    ushort numMessages = Math.Min((ushort)Math.Ceiling((float)contentLength / (float)maxContentLength),
                        _data.MaxMores);

                    for(ushort i = 0; i < numMessages; ++i)
                    {
                        int offset = i * maxContentLength;
                        //int length = Math.Min(message.Contents.Length)
                        yield return String.Concat(message.Contents.Substring(offset, maxContentLength),
                            MoreMessages((ushort)(numMessages - i - 1)));
                    }
                }
            }
            else
            {
                yield return message.Contents;
            }
        }

        private String MoreMessages(ushort num)
        {
            if(num == 0)
                return String.Empty;

            return ControlCodes.Bold(String.Concat(" (", num.ToString(), num == 1 ? " more message" : " more messages", 
                ")"));
        }

        private IMessageTarget ReplyTarget(IReceiveMessage message)
        {
            if(message.Receiver.Equals(message.Connection.Me))
                return message.Sender;
            else
                return message.Receiver;
        }

        private void LogUserError(IReceiveMessage message, Exception e)
        {
            _logger.InfoException("Error executing: \"" + message.Contents + "\".", e);
        }
    }
}
