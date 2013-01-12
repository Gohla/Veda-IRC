using System;
using System.Collections.Generic;
using Veda.Interface;

namespace Veda.Command
{
    public class NestedCommandNameHelper
    {
        private Dictionary<String, NestedCommandNameHelper> _nestedCommands =
            new Dictionary<String, NestedCommandNameHelper>(StringComparer.OrdinalIgnoreCase);

        public HashSet<ICommand> Commands { get; set; }
        public NestedCommandTypeHelper TypeHelper { get; set; }

        public NestedCommandNameHelper this[String s]
        {
            get
            {
                if(!_nestedCommands.ContainsKey(s))
                    _nestedCommands[s] = new NestedCommandNameHelper();
                return _nestedCommands[s];
            }
        }

        public NestedCommandNameHelper()
        {
            Commands = new HashSet<ICommand>();
            TypeHelper = new NestedCommandTypeHelper();
        }
    }
}
