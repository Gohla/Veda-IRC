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

        public IStorage Storage { get; set; }
        public IReceiveMessage Message { get; set; }
        public ICommand Command { get; set; }

        public IConversionContext ConversionContext { get; set; }
        public ushort CallDepth { get; set; }

        public IObservable<object> Evaluate(object result)
        {
            if(result == null)
                return Observable.Empty<object>();

            {
                String[] replies = result as String[];
                if(replies != null)
                    return Observable
                        .Defer<String>(() => Observable.Return(replies.ToString("; ")))
                        ;
            }

            {
                IEnumerable<String> replies = result as IEnumerable<String>;
                if(replies != null)
                    return Observable
                        .Defer<String>(() => Observable.Return(replies.ToString("; ")))
                        ;
            }

            {
                IObservable<IEnumerable<String>> replies = result as IObservable<IEnumerable<String>>;
                if(replies != null)
                    return replies
                        .Select(r => r.ToString("; "))
                        ;
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

            return Observable.Empty<object>();
        }
    }
}
