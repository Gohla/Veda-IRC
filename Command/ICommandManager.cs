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

        ICommand CreateCommand(String name, String displayName, MethodInfo method, object obj = null);
        ICommand CreateCommand<T>(String name, String displayName, Expression<T> expression);
        ICommandConverter CreateConverter<TTo>(Func<String, TTo> converter);
        ICommandConverter CreateConverter<TTo, TContext>(Func<String, TContext, TTo> converter);
    }
}
