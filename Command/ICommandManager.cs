using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Veda.Command
{
    public interface ICommandManager
    {
        IEnumerable<ICommand> GetCommands(String name);
        IEnumerable<ICommand> GetCommands(String name, int numArguments);
        IEnumerable<ICommand> GetCommands(String name, params Type[] argumentTypes);
        IEnumerable<ICommand> GetCommandsByArguments(params Type[] argumentTypes);
        IEnumerable<ICommand> GetCommandsByArgumentsExact(params Type[] argumentTypes);

        IParseResult Parse(String command);
        bool IsCommand(String command);

        Func<object> Call(String command, object conversionContext);
        Func<object> Call(String command, object conversionContext, params object[] commandContext);
        Func<object> CallUntyped(String name, object conversionContext, params String[] arguments);
        Func<object> CallUntyped(String name, object conversionContext, object[] commandContext, params String[] arguments);
        Func<object> CallTyped(String name, object conversionContext, params object[] arguments);

        void Add(ICommand command);
        void Remove(ICommand command);
        void Add(ICommandConverter converter);
        void Remove(ICommandConverter converter);
    }
}
