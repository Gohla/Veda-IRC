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
            object[] names = new[] { name };
            return ResolveNames(ref names);
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
            IEnumerable<ICommand> nameCandidates = ResolveNames(ref arguments);
            return ResolveTypes(conversionContext, arguments, nameCandidates);
        }

        private IEnumerable<ICommand> ResolveNames(ref object[] arguments)
        {
            IEnumerable<String> strings = arguments
                .TakeWhile(o => o.GetType().Equals(typeof(String)))
                .Select(o => o as String)
                ;
            if(strings.IsEmpty())
                throw new NoCommandNameException();

            // Find all command name matches and add them to a stack.
            Stack<Tuple<ushort, NestedCommandNameHelper>> matches = new Stack<Tuple<ushort, NestedCommandNameHelper>>();
            {
                NestedCommandNameHelper nameHelper = _commands.Root;
                ushort count = 0;
                foreach(String str in strings)
                {
                    ++count;
                    nameHelper = nameHelper[str];
                    if(nameHelper.TypeHelper.Valid)
                    {
                        matches.Push(Tuple.Create(count, nameHelper));
                    }
                }
            }

            // Pop matches from the stack until non-ambiguous commands are found. Stack enforces longest matching.
            Exception lastException = null;
            while(matches.Count != 0)
            {
                Tuple<ushort, NestedCommandNameHelper> match = matches.Pop();

                try
                {
                    // Check for a name ambiguity.
                    IPlugin[] ambiguity = match.Item2.Commands
                        .Select(c => c.Plugin)
                        .Distinct()
                        .ToArray()
                        ;
                    if(ambiguity.Length > 1)
                        throw new AmbiguousCommandsException(strings.ToString(" "), ambiguity);

                    arguments = arguments
                        .Skip(match.Item1)
                        .ToArray()
                        ;
                    return match.Item2.Commands;
                }
                catch(Exception e)
                {
                    lastException = e;
                }
            }

            if(lastException == null)
                throw new NoCommandException(strings.ToString(" "));
            throw lastException;
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
