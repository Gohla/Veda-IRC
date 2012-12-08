using System;
using System.Collections.Generic;
using Gohla.Shared;
using Veda.Command;

namespace Veda.Plugin
{
    public interface IPlugin : IDisposable, IKeyedObject<String>
    {
        String Name { get; }
        String Description { get; }
        IEnumerable<ICommand> Commands { get; }
    }
}
