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
        public String Name { get; private set; }

        public NoCommandException(String name)
            : base(CreateMessage(name))
        {
            Name = name;
        }

        public static String CreateMessage(String name)
        {
            return "Command with name " + name + " does not exist.";
        }
    }

    public class AmbiguousCommandsException : CommandException
    {
        public String Name { get; private set; }
        public IPlugin[] Candidates { get; private set; }

        public AmbiguousCommandsException(String name, IPlugin[] candidates)
            : base(CreateMessage(name, candidates))
        {
            Name = name;
            Candidates = candidates;
        }

        public static String CreateMessage(String name, IPlugin[] candidates)
        {
            return "Command with name " + name + " exists in multiple plugins: " + candidates.ToString(", ")
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
