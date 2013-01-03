using System;
using System.Collections.Generic;
using System.Linq;
using Gohla.Shared;
using Veda.Interface;

namespace Veda.Command
{
    public class CommandManager : ICommandManager
    {
        private MultiValueDictionary<String, ICommand> _ambiguousCommands =
            new MultiValueDictionary<String, ICommand>(StringComparer.OrdinalIgnoreCase);
        private MultiValueDictionary<IPlugin, ICommand> _pluginCommands =
            new MultiValueDictionary<IPlugin, ICommand>();
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
            Add(CommandBuilder.CreateConverter<String, Boolean>(s => Boolean.Parse(s)));
            Add(CommandBuilder.CreateConverter<String, Char>(s => Char.Parse(s)));
            Add(CommandBuilder.CreateConverter<String, UInt16>(s => UInt16.Parse(s)));
            Add(CommandBuilder.CreateConverter<String, UInt32>(s => UInt32.Parse(s)));
            Add(CommandBuilder.CreateConverter<String, UInt64>(s => UInt64.Parse(s)));
            Add(CommandBuilder.CreateConverter<String, Int16>(s => Int16.Parse(s)));
            Add(CommandBuilder.CreateConverter<String, Int32>(s => Int32.Parse(s)));
            Add(CommandBuilder.CreateConverter<String, Int64>(s => Int64.Parse(s)));
            Add(CommandBuilder.CreateConverter<String, Single>(s => Single.Parse(s)));
            Add(CommandBuilder.CreateConverter<String, Double>(s => Double.Parse(s)));
        }

        public IEnumerable<ICommand> GetCommands(String name)
        {
            return _ambiguousCommands.Get(name);
        }

        public IEnumerable<ICommand> GetUnambigousCommands(String name)
        {
            object[] dummy;
            return ResolveNames(new object[] { name }, true, out dummy);
        }

        public ICommand GetUnambigousCommand(String name)
        {
            return GetUnambigousCommands(name).Single();
        }

        public IEnumerable<ICommand> GetCommands(IPlugin plugin)
        {
            return _pluginCommands.Get(plugin);
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

        public ICallable Call(String command, object conversionContext = null)
        {
            String[] result = Parse(command);
            if(result == null)
                return null;
            return CallParsed(conversionContext, result);
        }

        public ICallable CallParsed(object conversionContext, params object[] arguments)
        {
            return Resolve(conversionContext, arguments);
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

            _ambiguousCommands.Add(command.Name, command);
            _pluginCommands.Add(command.Plugin, command);
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

            _ambiguousCommands.Remove(command.Name, command);
            _pluginCommands.Remove(command.Plugin, command);
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

        private ICallable Resolve(object conversionContext, params object[] arguments)
        {
            try
            {
                object[] newArguments;
                IEnumerable<ICommand> nameCandidates = ResolveNames(arguments, true, out newArguments);
                return ResolveTypes(conversionContext, newArguments, nameCandidates);
            }
            catch(Exception e)
            {
                object[] newArguments;
                IEnumerable<ICommand> nameCandidates;
                try
                {
                    nameCandidates = ResolveNames(arguments, false, out newArguments);
                }
                catch
                {
                    throw e;
                }
                return ResolveTypes(conversionContext, newArguments, nameCandidates);
            }
        }

        private IEnumerable<ICommand> ResolveNames(object[] arguments, bool qualify, out object[] newArguments)
        {
            if(arguments.Length == 0)
            {
                throw new ArgumentException("No command name was given.", "arguments");
            }
            else if(arguments.Length == 1)
            {
                IEnumerable<ICommand> candidates = GetCommands((String)arguments[0]);
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
                if(qualify)
                {
                    IEnumerable<ICommand> candidates = ResolveQualifiedNames(arguments);
                    if(candidates.IsEmpty())
                    {
                        throw new ArgumentException("Command with name " + arguments[1]
                            + " does not exist in plugin " + arguments[0] + ".",
                            "arguments");
                    }
                    newArguments = arguments.Skip(2).ToArray(); // TODO: More efficient array skipping.
                    return candidates;
                }
                else
                {
                    IEnumerable<ICommand> candidates = ResolveUnqualifiedNames(arguments);
                    if(candidates.IsEmpty())
                    {
                        throw new ArgumentException("Command with name " + arguments[0] + " does not exist.",
                            "arguments");
                    }
                    newArguments = arguments.Skip(1).ToArray(); // TODO: More efficient array skipping.
                    return candidates;
                }
            }
        }

        private IEnumerable<ICommand> ResolveUnqualifiedNames(object[] arguments)
        {
            IEnumerable<ICommand> candidates = GetCommands((String)arguments[0]);
            if(!candidates.IsEmpty())
            {
                // Check for a name ambiguity.
                IPlugin[] ambiguity = candidates.Select(c => c.Plugin).Distinct().ToArray();
                if(ambiguity.Length > 1)
                {
                    throw new ArgumentException("Command with name " + arguments[0]
                        + " exists in multiple plugins: " + ambiguity.ToString(", ") + ". ",
                        "arguments");
                }
                else
                {
                    return candidates;
                }
            }

            return Enumerable.Empty<ICommand>();
        }

        private IEnumerable<ICommand> ResolveQualifiedNames(object[] arguments)
        {
            IEnumerable<ICommand> candidates = GetCommands((String)arguments[0], (String)arguments[1]);
            if(!candidates.IsEmpty())
            {
                return candidates;
            }

            return Enumerable.Empty<ICommand>();
        }

        private ICallable ResolveTypes(object conversionContext, object[] arguments, 
            IEnumerable<ICommand> nameCandidates)
        {
            // If there are more arguments than the longest command can handle, concatenate the rest of the string 
            // arguments into one.
            // TODO: Efficient lookup
            int maxParamCount = nameCandidates.Max(c => c.ParameterTypes.Length);
            if(maxParamCount > 0 && arguments.Length > maxParamCount)
            {
                String joined = String.Join(" ", arguments.Skip(maxParamCount - 1));
                Array.Resize(ref arguments, maxParamCount);
                arguments[maxParamCount - 1] = joined;
            }

            // TODO: Efficient lookup
            IEnumerable<ICommand> typeCandidates = nameCandidates
                .Where(c => c.ParameterTypes.Length == arguments.Length);

            // TODO: Taking the first one that succeeds, may need something smarter though?
            foreach(ICommand command in typeCandidates)
            {
                object[] args = ConvertAll(conversionContext, arguments, command.ParameterTypes.ToArray());
                if(args != null)
                    return new Callable(command, args);
            }

            throw new InvalidOperationException("Incorrect arguments. Usage: " + nameCandidates.ToString("; "));
        }

        private object ConvertOne(object commandContext, object obj, Type target)
        {
            if(target.IsAssignableFrom(typeof(String)))
                return obj;

            // TODO: Efficient lookup
            IEnumerable<ICommandConverter> candidates;
            IEnumerable<ICommandConverter> candidatesNoContext = _converters
                .Where(c => target.IsAssignableFrom(c.ToType))
                .Where(c => c.ContextType.Equals(typeof(void)));
            if(commandContext != null)
            {
                candidates = Enumerable.Concat(
                    _converters
                        .Where(c => target.IsAssignableFrom(c.ToType))
                        .Where(c => c.ContextType.IsAssignableFrom(commandContext.GetType())),
                    candidatesNoContext
                );
            }
            else
            {
                candidates = candidatesNoContext;
            }

            // TODO: Taking the first one that succeeds, may need something smarter though?
            foreach(ICommandConverter converter in candidates)
            {
                try
                {
                    object converted = converter.Convert(obj, commandContext);
                    if(converted != null)
                        return converted;
                }
                catch { }
            }

            return null;
        }

        private object[] ConvertAll(object commandContext, object[] objs, params Type[] types)
        {
            if(objs.Length != types.Length)
                return null;

            object[] converted = new object[objs.Length];

            for(int i = 0; i < objs.Length; ++i)
            {
                converted[i] = ConvertOne(commandContext, objs[i], types[i]);
                if(converted[i] == null)
                    return null;
            }

            return converted;
        }
    }
}
