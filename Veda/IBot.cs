using System;
using System.Collections.ObjectModel;
using ReactiveIRC.Interface;

namespace Veda
{
    public interface IBot : IDisposable
    {
        ObservableCollection<IClientConnection> Connections { get; }

        IClientConnection Connect(Veda.Configuration.ConnectionData data);
    }
}
