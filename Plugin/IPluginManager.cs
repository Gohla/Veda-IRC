using System;
using System.Collections.Generic;

namespace Veda.Plugin
{
    public interface IPluginManager : IDisposable
    {
        IEnumerable<IPlugin> Plugins { get; }

        IPlugin Load(IPlugin plugin);
        IPlugin Get(String name);
        void Unload(String name);
    }
}
