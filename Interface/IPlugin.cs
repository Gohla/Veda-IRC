using System;
using System.Collections.Generic;
using Gohla.Shared;

namespace Veda.Interface
{
    public interface IPlugin : IDisposable, IEquatable<IPlugin>
    {
        String Name { get; }
        String Description { get; }
        IEnumerable<ICommand> InitialCommands { get; }
        object Instance { get; }
    }
}
