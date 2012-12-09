using System;
using System.Collections.Generic;

namespace Veda.Plugin
{
    public interface IPluginManager : IDisposable
    {
        IEnumerable<IPlugin> Plugins { get; }

        IPlugin Load(IPlugin plugin);
        IPlugin Load(Type type);
        IPlugin Get(String name);
        void Unload(String name);
    }
}
