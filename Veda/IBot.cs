using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;
using Veda.Configuration;

namespace Veda
{
    public interface IBot : IDisposable
    {
        IEnumerable<IClientConnection> Connections { get; }

        IClientConnection Connect(ConnectionData data);
    }
}
