using System;
using System.Collections.Generic;
using System.Linq;
using Gohla.Shared;
using Veda.Interface;

namespace Veda.Command
{
    public class CommandManager : ICommandManager
    {
        private NestedCommandHelper _commands = new NestedCommandHelper();
        private List<ICommandConverter> _converters = new List<ICommandConverter>();
        private ICommandParser _parser;

        private NestedCommandNameHelper _nestedCommandHelper = new NestedCommandNameHelper();

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
            return _commands.Get(name);
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
            return _commands.Get(plugin);
        }

        public IEnumerable<ICommand> GetCommands(String pluginName, String name)
        {
            return _commands.Get(pluginName, name);
        }

        public ICommand GetCommand(String pluginName, String name, params Type[] argumentTypes)
        {
            return _commands.Get(pluginName, name, argumentTypes);
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
            _commands.Add(command);
        }

        public void Remove(ICommand command)
        {
            _commands.Remove(command);
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
                throw new NoCommandNameException();
            }
            else if(arguments.Length == 1)
            {
                IEnumerable<ICommand> candidates = ResolveUnqualifiedNames(arguments[0].ToString());
                newArguments = arguments.Skip(1).ToArray(); // TODO: More efficient array skipping.
                return candidates;
            }
            else
            {
                if(qualify)
                {
                    IEnumerable<ICommand> candidates = ResolveQualifiedNames(arguments[0].ToString(), arguments[1].ToString());
                    newArguments = arguments.Skip(2).ToArray(); // TODO: More efficient array skipping.
                    return candidates;
                }
                else
                {
                    IEnumerable<ICommand> candidates = ResolveUnqualifiedNames(arguments[0].ToString());
                    newArguments = arguments.Skip(1).ToArray(); // TODO: More efficient array skipping.
                    return candidates;
                }
            }
        }

        private IEnumerable<ICommand> ResolveUnqualifiedNames(String commandName)
        {
            IEnumerable<ICommand> candidates = GetCommands(commandName);
            if(!candidates.IsEmpty())
            {
                // Check for a name ambiguity.
                IPlugin[] ambiguity = candidates
                    .Select(c => c.Plugin)
                    .Distinct()
                    .ToArray()
                    ;
                if(ambiguity.Length > 1)
                    throw new AmbiguousCommandsException(commandName, ambiguity);
                else
                    return candidates;
            }
            else
                throw new NoCommandException(commandName);
        }

        private IEnumerable<ICommand> ResolveQualifiedNames(String pluginName, String commandName)
        {
            IEnumerable<ICommand> candidates = GetCommands(pluginName, commandName);
            if(!candidates.IsEmpty())
                return candidates;
            else
                throw new NoCommandException(commandName, pluginName);
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
