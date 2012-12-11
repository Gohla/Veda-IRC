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

        public object Call()
        {
            return Command.Call(Arguments);
        }
    }
}
