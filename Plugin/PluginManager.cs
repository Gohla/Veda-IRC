using System;
using System.Collections.Generic;
using System.Linq;
using Gohla.Shared;
using Veda.Interface;

namespace Veda.Plugin
{
    public class PluginManager : IPluginManager
    {
        private ICommandManager _commandManager;
        private Dictionary<String, IPlugin> _plugins = 
            new Dictionary<String, IPlugin>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<IPlugin> Plugins { get { return _plugins.Values; } }

        public PluginManager(ICommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        public void Dispose()
        {
            if(_plugins == null)
                return;

            _plugins.ToArray().Do(x => Unload(x.Key));
            _plugins.Clear();
            _plugins = null;
        }

        public IPlugin Load(IPlugin plugin)
        {
            if(_plugins.ContainsKey(plugin.Name))
                throw new ArgumentException("Plugin with name " + plugin.Name + " is already loaded.", "plugin");

            _plugins.Add(plugin.Name, plugin);
            foreach(ICommand command in plugin.Commands)
                _commandManager.Add(command);

            return plugin;
        }

        public IPlugin Load(Type type)
        {
            IPlugin plugin = PluginScanner.Scan(type);
            if(plugin == null)
                throw new ArgumentException("Type " + plugin.Name + " is not a valid plugin.", "type");

            return Load(plugin);
        }

        public IPlugin Get(String name)
        {
            if(!_plugins.ContainsKey(name))
                throw new ArgumentException("Plugin with name " + name + " does not exist.", "name");

            return _plugins[name];
        }

        public void Unload(String name)
        {
            if(!_plugins.ContainsKey(name))
                throw new ArgumentException("Plugin with name " + name + " is not loaded.", "name");

            IPlugin plugin = _plugins[name];
            foreach(ICommand command in _plugins[name].Commands)
                _commandManager.Remove(command);
            _plugins.Remove(name);
            plugin.Dispose();
        }
    }
}
