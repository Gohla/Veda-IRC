using System;
using System.Collections.Generic;
using System.Linq;
using Veda.Interface;
using Gohla.Shared;

namespace Veda.Command
{
    public class NestedCommandHelper
    {
        private MultiValueDictionary<String, ICommand> _pluginCommands =
            new MultiValueDictionary<String, ICommand>(StringComparer.OrdinalIgnoreCase);

        public NestedCommandNameHelper Root { get; private set; }
        public HashSet<ICommand> AllCommands { get; private set; }

        public NestedCommandHelper()
        {
            Root = new NestedCommandNameHelper();
            AllCommands = new HashSet<ICommand>();
        }

        public void Add(ICommand command)
        {
            NestedCommandNameHelper qualifiedNameHelper = GetNamed(Root, command.Plugin.Name, command.Name);
            NestedCommandTypeHelper qualifiedTypeHelper = GetTyped(qualifiedNameHelper, command.ParameterTypes);

            if(qualifiedTypeHelper.Command != null)
                throw new ArgumentException(
                    "Command from plugin " + command.Plugin.Name
                    + " with name " + command.Name
                    + " and parameter types " + command.ParameterTypes.Select(t => t.Name).ToString(", ")
                    + " already exists.",
                    "command");

            NestedCommandNameHelper unqualifiedNameHelper = GetNamed(Root, command.Name);
            NestedCommandTypeHelper unqualifiedTypeHelper = GetTyped(unqualifiedNameHelper, command.ParameterTypes);

            qualifiedNameHelper.Commands.Add(command);
            qualifiedTypeHelper.Command = command;

            unqualifiedNameHelper.Commands.Add(command);
            unqualifiedTypeHelper.Command = command;

            _pluginCommands.Add(command.Plugin.Name, command);
            AllCommands.Add(command);
        }

        public void Remove(ICommand command)
        {
            if(!AllCommands.Contains(command))
                throw new ArgumentException(
                    "Command from plugin " + command.Plugin.Name
                    + " with name " + command.Name
                    + " and parameter types " + command.ParameterTypes.Select(t => t.Name).ToString(", ")
                    + " does not exists.",
                    "command");

            NestedCommandNameHelper qualifiedNameHelper = GetNamed(Root, command.Plugin.Name, command.Name);
            NestedCommandTypeHelper qualifiedTypeHelper = GetTyped(qualifiedNameHelper, command.ParameterTypes);
            NestedCommandNameHelper unqualifiedNameHelper = GetNamed(Root, command.Name);
            NestedCommandTypeHelper unqualifiedTypeHelper = GetTyped(unqualifiedNameHelper, command.ParameterTypes);

            qualifiedNameHelper.Commands.Remove(command);
            qualifiedTypeHelper.Command = null;

            unqualifiedNameHelper.Commands.Remove(command);
            unqualifiedTypeHelper.Command = null;

            _pluginCommands.Remove(command.Plugin.Name, command);
            AllCommands.Remove(command);
        }

        public IEnumerable<ICommand> Get(IPlugin plugin)
        {
            return _pluginCommands[plugin.Name];
        }

        public IEnumerable<ICommand> Get(String name)
        {
            return GetNamed(Root, name).Commands;
        }

        public IEnumerable<ICommand> Get(String pluginName, String name)
        {
            return GetNamed(Root, pluginName, name).Commands;
        }

        public IEnumerable<ICommand> Get(IEnumerable<String> names)
        {
            return GetNamed(Root, names).Commands;
        }

        public IEnumerable<ICommand> Get(String pluginName, IEnumerable<String> names)
        {
            return GetNamed(Root[pluginName], names).Commands;
        }

        public ICommand Get(String name, Type[] types)
        {
            return GetTyped(GetNamed(Root, name), types).Command;
        }

        public ICommand Get(String pluginName, String name, IEnumerable<Type> types)
        {
            return GetTyped(GetNamed(Root, pluginName, name), types).Command;
        }

        public ICommand Get(IEnumerable<String> names, IEnumerable<Type> types)
        {
            return GetTyped(GetNamed(Root, names), types).Command;
        }

        public ICommand Get(String pluginName, IEnumerable<String> names, IEnumerable<Type> types)
        {
            return GetTyped(GetNamed(Root[pluginName], names), types).Command;
        }

        private NestedCommandNameHelper GetNamed(NestedCommandNameHelper nameHelper, String name)
        {
            return GetNamed(nameHelper, name.Split(' '));
        }

        private NestedCommandNameHelper GetNamed(NestedCommandNameHelper nameHelper, String pluginName, String name)
        {
            return GetNamed(nameHelper[pluginName], name);
        }

        private NestedCommandNameHelper GetNamed(NestedCommandNameHelper nameHelper, IEnumerable<String> names)
        {
            foreach(String subname in names)
                nameHelper = nameHelper[subname];

            return nameHelper;
        }

        private NestedCommandTypeHelper GetTyped(NestedCommandNameHelper nameHelper, IEnumerable<Type> types)
        {
            NestedCommandTypeHelper typeHelper = nameHelper.TypeHelper;
            foreach(Type type in types)
                typeHelper = typeHelper[type];

            return typeHelper;
        }
    }
}
