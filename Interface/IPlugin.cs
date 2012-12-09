using System;
using System.Collections.Generic;
using Gohla.Shared;

namespace Veda.Interface
{
    public interface IPlugin : IDisposable, IKeyedObject<String>
    {
        String Name { get; }
        String Description { get; }
        IEnumerable<ICommand> Commands { get; }
        object Instance { get; }
    }
}
