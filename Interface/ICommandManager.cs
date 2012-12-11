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

        ICallable Call(String command, object conversionContext);
        ICallable Call(String command, object conversionContext, params object[] commandContext);
        ICallable CallUntyped(object conversionContext, params String[] arguments);
        ICallable CallUntyped(object conversionContext, object[] commandContext, params String[] arguments);

        void Add(ICommand command);
        void Remove(ICommand command);
        void Add(ICommandConverter converter);
        void Remove(ICommandConverter converter);
    }
}
