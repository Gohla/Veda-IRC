using System;
using System.Collections.Generic;

namespace Veda.Interface
{
    public class CommandException : Exception
    {
        public CommandException(String message)
            : base(message)
        {

        }
    }

    public class NoCommandNameException : CommandException
    {
        public NoCommandNameException()
            : base("No command name was given.")
        {

        }
    }

    public class NoCommandException : CommandException
    {
        public String CommandName { get; private set; }
        public String PluginName { get; private set; }

        public NoCommandException(String commandName, String pluginName = null)
            : base(CreateMessage(commandName, pluginName))
        {
            CommandName = commandName;
            PluginName = pluginName;
        }

        public static String CreateMessage(String commandName, String pluginName)
        {
            if(String.IsNullOrWhiteSpace(pluginName))
                return "Command with name " + commandName + " does not exist.";
            else
                return "Command with name " + commandName + " does not exist in plugin " + pluginName + ".";
        }
    }

    public class AmbiguousCommandsException : CommandException
    {
        public String CommandName { get; private set; }
        public IPlugin[] Candidates { get; private set; }

        public AmbiguousCommandsException(String commandName, IPlugin[] candidates)
            : base(CreateMessage(commandName, candidates))
        {
            CommandName = commandName;
            Candidates = candidates;
        }

        public static String CreateMessage(String commandName, IPlugin[] candidates)
        {
            return "Command with name " + commandName + " exists in multiple plugins: " + candidates.ToString(", ")
                + ".";
        }
    }

    public class IncorrectArgumentsException : CommandException
    {
        public IEnumerable<ICommand> Candidates { get; private set; }

        public IncorrectArgumentsException(IEnumerable<ICommand> candidates)
            : base(CreateMessage(candidates))
        {
            Candidates = candidates;
        }

        public static String CreateMessage(IEnumerable<ICommand> candidates)
        {
            return "Incorrect arguments. Usage: " + candidates.ToString("; ");
        }
    }
}
