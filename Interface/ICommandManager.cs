using System;
using System.Collections.Generic;

namespace Veda.Interface
{
    public interface ICommandManager
    {
        IEnumerable<ICommand> GetCommands(String name);
        IEnumerable<ICommand> GetUnambigousCommands(String name);
        IEnumerable<ICommand> GetCommands(String pluginName, String name);
        IEnumerable<ICommand> GetCommands(String name, params Type[] argumentTypes);
        ICommand GetCommand(String pluginName, String name, params Type[] argumentTypes);

        String[] Parse(String command);
        bool IsCommand(String command);

        Func<object> Call(String command, object conversionContext);
        Func<object> Call(String command, object conversionContext, params object[] commandContext);
        Func<object> CallUntyped(object conversionContext, params String[] arguments);
        Func<object> CallUntyped(object conversionContext, object[] commandContext, params String[] arguments);

        void Add(ICommand command);
        void Remove(ICommand command);
        void Add(ICommandConverter converter);
        void Remove(ICommandConverter converter);
    }
}
