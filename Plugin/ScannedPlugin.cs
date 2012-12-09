using System;
using System.Collections.Generic;
using Veda.Command;

namespace Veda.Plugin
{
    public class ScannedPlugin : IPlugin
    {
        private Action _dispose;

        public String Name { get; set; }
        public String Description { get; set; }
        public IEnumerable<ICommand> Commands { get; set; }
        public object Instance { get; set; }
        public String Key { get { return Name; } }

        public ScannedPlugin(String name, String description, IEnumerable<ICommand> commands, object instance, 
            Action dispose)
        {
            Name = name;
            Description = description;
            Commands = commands;
            Instance = instance;
            _dispose = dispose;
        }

        public void Dispose()
        {
            if(_dispose != null)
                _dispose();
        }
    }
}
