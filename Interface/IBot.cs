using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IBot : IDisposable
    {
        IEnumerable<IClientConnection> Connections { get; }
        IObservable<Tuple<IContext, IReceiveMessage>> Messages { get; }

        IPluginStorageManager Storage { get; }
        ICommandManager Command { get; }
        IPluginManager Plugin { get; }
        IAuthenticationManager Authentication { get; }
        IPluginPermissionManager Permission { get; }

        IClientConnection Connect(String address, ushort port, String nickname, String username, String realname,
            String password);
        String More();
        String More(IMessageTarget sender);
        void Output(IContext context, IReceiveMessage message, IObservable<object> output, bool replySuccess = true);
    }
}