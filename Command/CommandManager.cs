using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gohla.Shared;

namespace Veda.Command
{
    public class CommandManager : ICommandManager
    {
        private List<ICommand> _commands = new List<ICommand>();
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
            // TODO: Efficient lookup
            return _commands
                .Where(c => c.Name == name)
                ;
        }

        public IEnumerable<ICommand> GetCommands(String name, int numArguments)
        {
            // TODO: Efficient lookup
            return GetCommands(name)
                .Where(c => c.ParameterTypes.Length == numArguments)
                ;
        }

        public IEnumerable<ICommand> GetCommands(String name, params Type[] argumentTypes)
        {
            // TODO: Efficient lookup
            return GetCommands(name)
                .Where(c => c.IsCompatible(argumentTypes))
                ;
        }

        public IEnumerable<ICommand> GetCommandsByArguments(params Type[] argumentTypes)
        {
            // TODO: Efficient lookup
            return _commands
                .Where(c => c.IsPartialCompatible(argumentTypes))
                ;
        }

        public IEnumerable<ICommand> GetCommandsByArgumentsExact(params Type[] argumentTypes)
        {
            // TODO: Efficient lookup
            return _commands
                .Where(c => c.IsCompatible(argumentTypes))
                ;
        }

        public IParseResult Parse(String command)
        {
            return _parser.Parse(command);
        }

        public bool IsCommand(String command)
        {
            IParseResult result = _parser.Parse(command);
            if(result == null)
                return false;

            return true;
        }

        public Func<object> Call(String command, object conversionContext)
        {
            IParseResult result = _parser.Parse(command);
            return CallUntyped(result.Name, conversionContext, result.Arguments);
        }

        public Func<object> Call(String command, object conversionContext, params object[] commandContext)
        {
            IParseResult result = _parser.Parse(command);
            return CallUntyped(result.Name, conversionContext, commandContext, result.Arguments);
        }

        public Func<object> CallUntyped(String name, object conversionContext, params String[] arguments)
        {
            return Resolve(name, conversionContext, new object[0], arguments);
        }

        public Func<object> CallUntyped(String name, object conversionContext, object[] commandContext, 
            params String[] arguments)
        {
            return Resolve(name, conversionContext, commandContext, arguments);
        }

        public Func<object> CallTyped(String name, object conversionContext, params object[] arguments)
        {
            // TODO: Efficient lookup
            // TODO: Taking the first one that succeeds, may need something smarter though?
            return () => GetCommands(name, arguments.Select(o => o.GetType()).ToArray()).First().Call(arguments);
        }

        public void Add(ICommand command)
        {
            _commands.Add(command);
        }

        public void Remove(ICommand command)
        {
            // TODO: Efficient removal
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

        private Func<object> Resolve(String name, object conversionContext, object[] commandContext, 
            params String[] arguments)
        {
            IEnumerable<ICommand> commandsByName = GetCommands(name);
            if(!commandsByName.Any())
                return null;

            // If there are more arguments than the longest command can handle, concatenate the rest of the string 
            // arguments into one.
            // TODO: Efficient lookup
            int maxParamCount = commandsByName.Max(c => c.ParameterTypes.Length);
            if(arguments.Length > maxParamCount)
            {
                String joined = String.Join(" ", arguments.Skip(maxParamCount - 1));
                Array.Resize(ref arguments, maxParamCount);
                arguments[maxParamCount - 1] = joined;
            }

            // TODO: Efficient lookup
            IEnumerable<ICommand> candidates = commandsByName
                .Where(c => c.ParameterTypes.Length == arguments.Length + commandContext.Length)
                .Where(c => c.IsPartialCompatible(commandContext.Select(o => o.GetType()).ToArray()));

            // TODO: Taking the first one that succeeds, may need something smarter though?
            foreach(ICommand command in candidates)
            {
                object[] args = ConvertAll(conversionContext, arguments, 
                    command.ParameterTypes.Skip(commandContext.Length).ToArray());
                if(args != null)
                    return () => command.Call(commandContext.Concat(args));
            }

            return null;
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
