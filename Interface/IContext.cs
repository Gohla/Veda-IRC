﻿using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IContext
    {
        IBot Bot { get; set; }
        IClientConnection Connection { get; set; }
        IReceiveMessage Message { get; set; }
    }
}