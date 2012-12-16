using System;
using System.Collections.Generic;
using System.Reflection;

namespace Veda.Interface
{
    public interface IPluginManager : IDisposable
    {
        IEnumerable<IPlugin> Plugins { get; }

        void Load(Assembly assembly, IBot bot);
        IPlugin Load(IPlugin plugin, IBot bot);
        IPlugin Load(Type type, IBot bot);
        IPlugin Get(String name);
        void Unload(String name);
    }
}
