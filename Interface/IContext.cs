﻿using System;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IContext
    {
        IBot Bot { get; }

        IReceiveMessage Message { get; }
        IStorage Storage { get; }
        IBotUser Sender { get; }
        ICommand Command { get; }

        IConversionContext ConversionContext { get; }
        ushort CallDepth { get; set; }

        IObservable<object> Evaluate(object result);
    }
}
