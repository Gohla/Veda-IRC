using System;
using NLog;
using ReactiveIRC.Interface;
using Veda.Configuration;
using Veda.Interface;

namespace Veda
{
    public class ReplyHandler
    {
        private static readonly Logger _logger = LogManager.GetLogger("ReplyHandler");

        private BotData _data;

        public ReplyHandler(BotData data)
        {
            _data = data;
        }

        public void Reply(IReceiveMessage message, IMessageTarget sender, ReplyForm replyForm, object result)
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
