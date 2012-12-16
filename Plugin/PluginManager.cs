﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Veda.Interface;

namespace Veda.Plugin
{
    public class PluginManager : IPluginManager
    {
        private static readonly Logger _logger = LogManager.GetLogger("PluginManager");

        private readonly ICommandManager _commandManager;
        private readonly IPluginStorageManager _pluginStorageManager;
        private Dictionary<String, IPlugin> _plugins = 
            new Dictionary<String, IPlugin>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<IPlugin> Plugins { get { return _plugins.Values; } }

        public PluginManager(ICommandManager commandManager, IPluginStorageManager pluginStorageManager)
        {
            _commandManager = commandManager;
            _pluginStorageManager = pluginStorageManager;
        }

        public void Dispose()
        {
            if(_plugins == null)
                return;

            _plugins.ToArray().Do(x => Unload(x.Key));
            _plugins.Clear();
            _plugins = null;
        }

        public void Load(Assembly assembly, IBot bot)
        {
            IEnumerable<IPlugin> plugins = PluginScanner.Scan(assembly);
            foreach(IPlugin plugin in plugins)
            {
                try
                {
                    Load(plugin, bot);
                }
                catch(Exception e)
                {
                    _logger.ErrorException("Unable to load plugin " + plugin.Name + ".", e);
                }
            }
        }

        public IPlugin Load(IPlugin plugin, IBot bot)
        {
            if(_plugins.ContainsKey(plugin.Name))
                throw new ArgumentException("Plugin with name " + plugin.Name + " is already loaded.", "plugin");

            _plugins.Add(plugin.Name, plugin);
            foreach(ICommand command in plugin.InitialCommands)
            {
                try
                {
                    _commandManager.Add(command);
                }
                catch(Exception e)
                {
                    _logger.ErrorException("Unable to add command " + command.Name + " from plugin " 
                        + plugin.Name + ".", e);
                }
            }

            LoadStorage(plugin, bot);

            return plugin;
        }

        public IPlugin Load(Type type, IBot bot)
        {
            IPlugin plugin = PluginScanner.Scan(type);
            if(plugin == null)
                throw new ArgumentException("Type " + plugin.Name + " is not a valid plugin.", "type");

            return Load(plugin, bot);
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
            foreach(ICommand command in _commandManager.GetCommands(plugin).ToArray())
                _commandManager.Remove(command);
            _plugins.Remove(name);
            plugin.Dispose();
        }

        private void LoadStorage(IPlugin plugin, IBot bot)
        {
            PropertyInfo[] properties = plugin.Type.GetProperties();
            IStorage storage = _pluginStorageManager.Global(plugin);

            foreach(PropertyInfo property in properties)
            {
                if(property.CanRead && property.CanWrite)
                {
                    if(storage.Exists(property.Name))
                    {
                        try
                        {
                            property.SetValue(plugin.Instance, storage.Get(property.Name, property.PropertyType));
                        }
                        catch(Exception e)
                        {
                            _logger.ErrorException("Could not set property " + property.Name + " in plugin "
                                + plugin.Name + ".", e);
                        }
                    }
                    else
                    {
                        try
                        {
                            storage.Set(property.Name, property.GetValue(plugin.Instance));
                        }
                        catch(Exception e)
                        {
                            _logger.ErrorException("Could not get property " + property.Name + " in plugin "
                                + plugin.Name + ".", e);
                        }
                    }
                }
            }

            if(plugin.Loaded != null)
            {
                try
                {
                    plugin.Loaded(plugin, bot);
                }
                catch(Exception e)
                {
                    _logger.ErrorException("Could not invoke loaded method in plugin " + plugin.Name + ".", e);
                }
            }
        }
    }
}
