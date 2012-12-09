using System;
using System.Collections.Generic;
using System.Reflection;

namespace Veda.Interface
{
    public interface IPluginManager : IDisposable
    {
        IEnumerable<IPlugin> Plugins { get; }

        void Load(Assembly assembly);
        IPlugin Load(IPlugin plugin);
        IPlugin Load(Type type);
        IPlugin Get(String name);
        void Unload(String name);
    }
}
