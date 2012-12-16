using System;
using Veda.Interface;

namespace Veda.Command
{
    public class Callable : ICallable
    {
        public ICommand Command { get; private set; }
        public object[] Arguments { get; private set; }

        public Callable(ICommand command, object[] arguments)
        {
            Command = command;
            Arguments = arguments;
        }

        public object Call(IContext context)
        {
            if(context.CallDepth > 10)
                throw new InvalidOperationException("Command recursing too deep, execution halted.");

            ++context.CallDepth;
            return Command.Call(context, Arguments);
        }
    }
}
