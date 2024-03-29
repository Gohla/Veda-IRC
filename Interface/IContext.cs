﻿using System;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public enum ReplyForm
    {
        Echo
      , Reply
      , Action
      , Notice
      , More
    }

    public interface IContext
    {
        IBot Bot { get; }

        IClientConnection Connection { get; }
        IUser Sender { get; }
        IChannel Channel { get; }
        IBotUser User { get; }
        String Contents { get; }
        IStorage Storage { get; }
        ICommand Command { get; set; }

        Action<ICommand> Allowed { get; }
        IConversionContext ConversionContext { get; }
        ushort CallDepth { get; set; }
        ReplyForm ReplyForm { get; set; }
        String Seperator { get; set; }

        IObservable<object> Evaluate(object result);
    }
}
