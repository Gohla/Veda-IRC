using System;
using System.Collections.Generic;
using System.Linq;
using Gohla.Shared;
using Veda.Interface;

namespace Veda.Command
{
    public class CommandManager : ICommandManager
    {
        private MultiValueDictionary<String, ICommand> _commands =
            new MultiValueDictionary<String, ICommand>(StringComparer.OrdinalIgnoreCase);
        private MultiValueDictionary<QualifiedName, ICommand> _nameQualifiedCommands =
            new MultiValueDictionary<QualifiedName, ICommand>();
        private MultiValueDictionary<QualifiedTypes, ICommand> _typesQualifiedCommands =
            new MultiValueDictionary<QualifiedTypes, ICommand>();
        private Dictionary<QualifiedNameTypes, ICommand> _nameTypesQualifiedCommands =
            new Dictionary<QualifiedNameTypes, ICommand>();
        private List<ICommandConverter> _converters = new List<ICommandConverter>();
        private ICommandParser _parser;

        public CommandManager(ICommandParser parser)
        {
            _parser = parser;

            // Add default converters
            Add(CommandBuilder.CreateConverter<Char>(s => Char.Parse(s)));
            Add(CommandBuilder.CreateConverter<Int16>(s => Int16.Parse(s)));
            Add(CommandBuilder.CreateConverter<Int32>(s => Int32.Parse(s)));
            Add(CommandBuilder.CreateConverter<Int64>(s => Int64.Parse(s)));
            Add(CommandBuilder.CreateConverter<Single>(s => Single.Parse(s)));
            Add(CommandBuilder.CreateConverter<Double>(s => Double.Parse(s)));
        }

        public IEnumerable<ICommand> GetCommands(String name)
        {
            return _commands.Get(name);
        }

        public IEnumerable<ICommand> GetUnambigousCommands(String name)
        {
            String[] dummy;
            return NameCandidates(new[] { name }, out dummy);
        }

        public IEnumerable<ICommand> GetCommands(String pluginName, String name)
        {
            return _nameQualifiedCommands.Get(new QualifiedName(pluginName, name));
        }

        public IEnumerable<ICommand> GetCommands(String name, params Type[] argumentTypes)
        {
            return _typesQualifiedCommands.Get(new QualifiedTypes(name, argumentTypes));
        }

        public ICommand GetCommand(String pluginName, String name, params Type[] argumentTypes)
        {
            return _nameTypesQualifiedCommands[new QualifiedNameTypes(pluginName, name, argumentTypes)];
        }

        public String[] Parse(String command)
        {
            return _parser.Parse(command);
        }

        public bool IsCommand(String command)
        {
            String[] result = Parse(command);
            if(result == null)
                return false;

            return true;
        }

        public Func<object> Call(String command, object conversionContext)
        {
            String[] result = Parse(command);
            if(result == null)
                return null;
            return CallUntyped(conversionContext, result);
        }

        public Func<object> Call(String command, object conversionContext, params object[] commandContext)
        {
            String[] result = Parse(command);
            if(result == null)
                return null;
            return CallUntyped(conversionContext, commandContext, result);
        }

        public Func<object> CallUntyped(object conversionContext, params String[] arguments)
        {
            return Resolve(conversionContext, new object[0], arguments);
        }

        public Func<object> CallUntyped(object conversionContext, object[] commandContext, 
            params String[] arguments)
        {
            return Resolve(conversionContext, commandContext, arguments);
        }

        public void Add(ICommand command)
        {
            QualifiedNameTypes qualified = new QualifiedNameTypes(command.Plugin.Name, command.Name, 
                command.ParameterTypes);

            if(_nameTypesQualifiedCommands.ContainsKey(qualified))
                throw new ArgumentException(
                    "Command from plugin "    + command.Plugin.Name 
                    + " with name "           + command.Name 
                    + " and parameter types " + command.ParameterTypes.Select(t => t.Name).ToString(", ") 
                    + " already exists.", 
                    "command");

            _commands.Add(command.Name, command);
            _nameQualifiedCommands.Add(new QualifiedName(command.Plugin.Name, command.Name), command);
            _typesQualifiedCommands.Add(new QualifiedTypes(command.Name, command.ParameterTypes), command);
            _nameTypesQualifiedCommands.Add(qualified, command);
        }

        public void Remove(ICommand command)
        {
            QualifiedNameTypes qualified = new QualifiedNameTypes(command.Plugin.Name, command.Name, 
                command.ParameterTypes);

            if(!_nameTypesQualifiedCommands.ContainsKey(qualified))
                throw new ArgumentException(
                    "Command from plugin " + command.Plugin.Name
                    + " with name " + command.Name
                    + " and parameter types " + command.ParameterTypes.Select(t => t.Name).ToString(", ")
                    + " does not exist.",
                    "command");

            _commands.Remove(command.Name, command);
            _nameQualifiedCommands.Remove(new QualifiedName(command.Plugin.Name, command.Name), command);
            _typesQualifiedCommands.Remove(new QualifiedTypes(command.Name, command.ParameterTypes), command);
            _nameTypesQualifiedCommands.Remove(qualified);
        }

        public void Add(ICommandConverter converter)
        {
            _converters.Add(converter);
        }

        public void Remove(ICommandConverter converter)
        {
            // TODO: Efficient removal
            _converters.Remove(converter);
        }

        private IEnumerable<ICommand> NameCandidates(String[] arguments, out String[] newArguments)
        {
            if(arguments.Length == 0)
            {
                throw new ArgumentException("No command name was given.", "arguments");
            }
            else if(arguments.Length == 1)
            {
                IEnumerable<ICommand> candidates = GetCommands(arguments[0]);
                if(candidates.IsEmpty())
                    throw new ArgumentException("Command with name " + arguments[0] + " does not exist.", "arguments");

                // Check for a name ambiguity.
                IPlugin[] ambiguity = candidates.Select(c => c.Plugin).Distinct().ToArray();
                if(ambiguity.Length > 1)
                {
                    throw new ArgumentException("Command with name " + arguments[0]
                        + " exists in multiple plugins: " + ambiguity.ToString(", ") + ". ",
                        "arguments");
                }

                newArguments = arguments.Skip(1).ToArray(); // TODO: more efficient array skipping.
                return candidates;
            }
            else
            {
                // Try an unqualified name.
                IEnumerable<ICommand> candidates = GetCommands(arguments[0]);
                if(candidates.IsEmpty())
                {
                    // Try a qualified name.
                    IEnumerable<ICommand> qualifiedCandidates = GetCommands(arguments[0], arguments[1]);
                    if(qualifiedCandidates.IsEmpty())
                    {
                        throw new ArgumentException("Command with name " + arguments[0] 
                            + " does not exist in plugin " + arguments[1] + ".", 
                            "arguments");
                    }
                    else
                    {
                        newArguments = arguments.Skip(2).ToArray(); // TODO: more efficient array skipping.
                        return qualifiedCandidates;
                    }
                }
                else
                {
                    // Check for a name ambiguity.
                    IPlugin[] ambiguity = candidates.Select(c => c.Plugin).Distinct().ToArray();
                    if(ambiguity.Length > 1)
                    {
                        // There is a name ambiguity, try a qualified name.
                        IEnumerable<ICommand> qualifiedCandidates = GetCommands(arguments[0], arguments[1]);
                        if(qualifiedCandidates.IsEmpty())
                        {
                            throw new ArgumentException("Command with name " + arguments[0]
                                + " exists in multiple plugins: " + ambiguity.ToString(", ") + ". ",
                                "arguments");
                        }
                        else
                        {
                            // Name ambiguity can be resolved by qualifying the name.
                            newArguments = arguments.Skip(2).ToArray(); // TODO: more efficient array skipping.
                            return qualifiedCandidates;
                        }
                    }
                    else
                    {
                        // There is no name ambiguity.
                        newArguments = arguments.Skip(1).ToArray(); // TODO: more efficient array skipping.
                        return candidates;
                    }
                }
            }
        }

        private Func<object> Resolve(object conversionContext, object[] commandContext, params String[] arguments)
        {
            String[] newArguments;
            IEnumerable<ICommand> nameCandidates = NameCandidates(arguments, out newArguments);

            // If there are more arguments than the longest command can handle, concatenate the rest of the string 
            // arguments into one.
            // TODO: Efficient lookup
            int maxParamCount = nameCandidates.Max(c => c.ParameterTypes.Length);
            if(newArguments.Length > maxParamCount)
            {
                String joined = String.Join(" ", newArguments.Skip(maxParamCount));
                Array.Resize(ref newArguments, maxParamCount);
                newArguments[maxParamCount] = joined;
            }

            // TODO: Efficient lookup
            IEnumerable<ICommand> typeCandidates = nameCandidates
                .Where(c => c.ParameterTypes.Length == newArguments.Length + commandContext.Length)
                .Where(c => c.IsPartialCompatible(commandContext.Select(o => o.GetType()).ToArray()));

            // TODO: Taking the first one that succeeds, may need something smarter though?
            foreach(ICommand command in typeCandidates)
            {
                object[] args = ConvertAll(conversionContext, newArguments, 
                    command.ParameterTypes.Skip(commandContext.Length).ToArray());
                if(args != null)
                    return () => command.Call(commandContext.Concat(args));
            }

            throw new InvalidOperationException("Incorrect arguments. Usage: " + nameCandidates.ToString("; "));
        }

        private object ConvertOne(object commandContext, String str, Type target)
        {
            if(target.IsAssignableFrom(typeof(String)))
                return str;

            // TODO: Efficient lookup
            IEnumerable<ICommandConverter> candidates;
            if(commandContext == null)
                candidates = _converters
                    .Where(c => target.IsAssignableFrom(c.ToType))
                    .Where(c => c.ContextType.Equals(typeof(void)));
            else
                candidates = _converters
                    .Where(c => target.IsAssignableFrom(c.ToType))
                    .Where(c => c.ContextType.IsAssignableFrom(commandContext.GetType()));

            // TODO: Taking the first one that succeeds, may need something smarter though?
            foreach(ICommandConverter converter in candidates)
            {
                try
                {
                    object converted = converter.Convert(str, commandContext);
                    if(converted != null)
                        return converted;
                }
                catch { }
            }

            return null;
        }

        private object[] ConvertAll(object commandContext, String[] strs, params Type[] types)
        {
            if(strs.Length != types.Length)
                return null;

            object[] objs = new object[strs.Length];

            for(int i = 0; i < strs.Length; ++i)
            {
                objs[i] = ConvertOne(commandContext, strs[i], types[i]);
                if(objs[i] == null)
                    return null;
            }

            return objs;
        }
    }
}
