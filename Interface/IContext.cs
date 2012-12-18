using System;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IContext
    {
        IBot Bot { get; }

        IClientConnection Connection { get; }
        IUser Sender { get; }
        IChannel Channel { get; }
        IBotUser User { get; }
        String Contents { get; }
        IStorage Storage { get; }
        ICommand Command { get; }

        IConversionContext ConversionContext { get; }
        ushort CallDepth { get; set; }

        IObservable<object> Evaluate(object result);
    }
}
