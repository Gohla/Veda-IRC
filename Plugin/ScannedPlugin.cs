using System;
using System.Collections.Generic;
using Veda.Interface;

namespace Veda.Plugin
{
    public class ScannedPlugin : IPlugin
    {
        private List<ICommand> _commands = new List<ICommand>();
        private Action _dispose;

        public String Name { get; set; }
        public String Description { get; set; }
        public IEnumerable<ICommand> InitialCommands { get { return _commands; } }
        public object Instance { get; set; }

        public ScannedPlugin(String name, String description, object instance = null, Action dispose = null)
        {
            Name = name;
            Description = description;
            Instance = instance;
            _dispose = dispose;
        }

        public void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }

        public void AddCommands(IEnumerable<ICommand> commands)
        {
            _commands.AddRange(commands);
        }

        public void Dispose()
        {
            if(_dispose != null)
                _dispose();
        }

        public override bool Equals(object other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return Equals(other as IPlugin);
        }

        public bool Equals(IPlugin other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return
                StringComparer.OrdinalIgnoreCase.Equals(this.Name, other.Name)
            ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name);
                return hash;
            }
        }

        public override String ToString()
        {
            return this.Name;
        }
    }
}
