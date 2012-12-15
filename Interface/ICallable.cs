using System;

namespace Veda.Interface
{
    public interface ICallable
    {
        ICommand Command { get; }
        object[] Arguments { get; }

        object Call(IContext context);
    }
}
