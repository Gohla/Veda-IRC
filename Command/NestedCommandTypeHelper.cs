﻿using System;
using System.Collections.Generic;
using Veda.Interface;

namespace Veda.Command
{
    public class NestedCommandTypeHelper
    {
        private Dictionary<Type, NestedCommandTypeHelper> _nestedCommands =
            new Dictionary<Type, NestedCommandTypeHelper>();

        public ICommand Command { get; set; }
        public bool Valid
        {
            get
            {
                return Command != null || _nestedCommands.Count > 0;
            }
        }

        public NestedCommandTypeHelper this[Type t]
        {
            get
            {
                if(!_nestedCommands.ContainsKey(t))
                    _nestedCommands[t] = new NestedCommandTypeHelper();
                return _nestedCommands[t];
            }
        }
    }
}
