using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda
{
    internal class Context : IContext
    {
        public IBot Bot { get; set; }

        public IClientConnection Connection { get; set; }
        public IUser Sender { get; set; }
        public IChannel Channel { get; set; }
        public IBotUser User { get; set; }
        public String Contents { get; set; }
        public IStorage Storage
        {
            get
            {
                return Bot.Storage.PluginStorage(Command.Plugin, Connection, Channel);
            }
        }
        public ICommand Command { get; set; }

        public Action<ICommand> Allowed { get; set; }
        public IConversionContext ConversionContext { get; set; }
        public ushort CallDepth { get; set; }
        public ReplyForm ReplyForm { get; set; }
        public String Seperator { get; set; }

        public IObservable<object> Evaluate(object result)
        {
            if(result == null)
                return Observable.Empty<object>();

            {
                String[] replies = result as String[];
                if(replies != null)
                    return Observable
                        .Defer<String>(() => Observable.Return(replies.ToString(Seperator)))
                        ;
            }

            {
                IEnumerable<String> replies = result as IEnumerable<String>;
                if(replies != null)
                    return Observable
                        .Defer<String>(() => Observable.Return(replies.ToString(Seperator)))
                        ;
            }

            {
                IObservable<IEnumerable<String>> replies = result as IObservable<IEnumerable<String>>;
                if(replies != null)
                    return replies
                        .Select(r => r.ToString(Seperator))
                        ;
            }

            {
                IEnumerable<object> replies = result as IEnumerable<object>;
                if(replies != null)
                    return replies.ToObservable();
            }

            {
                IObservable<object> replies = result as IObservable<object>;
                if(replies != null)
                    return replies;
            }

            {
                ICallable callable = result as ICallable;
                if(callable != null)
                    return Observable
                        .Defer(() => Evaluate(callable.Call(this)))
                        ;
            }

            {
                Exception exception = result as Exception;
                if(exception != null)
                    return Observable.Throw<String>(exception);
            }

            {
                String reply = result as String;
                if(!String.IsNullOrWhiteSpace(reply))
                    return Observable.Return(reply);
            }

            return Observable.Return<object>(result);
        }
    }
}
