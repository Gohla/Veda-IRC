using System;
using System.Collections.Generic;

namespace Veda.Interface
{
    public interface ICommandManager
    {
        IEnumerable<ICommand> GetCommands(String name);
        IEnumerable<ICommand> GetUnambigousCommands(String name);
        ICommand GetUnambigousCommand(String name);
        IEnumerable<ICommand> GetCommands(IPlugin plugin);
        IEnumerable<ICommand> GetCommands(String pluginName, String name);
        ICommand GetCommand(String pluginName, String name, params Type[] argumentTypes);

        IExpression Parse(String command);
        bool IsCommand(String command);

        ICallable Call(String command, params object[] arguments);
        ICallable CallParsed(object conversionContext, params object[] arguments);

        void Add(ICommand command);
        void Remove(ICommand command);
        void Add(ICommandConverter converter);
        void Remove(ICommandConverter converter);
    }
}
