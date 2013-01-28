using System;

namespace Veda.Interface
{
    public interface ICallable
    {
        object[] Arguments { get; }

        object Call(IContext context, Action<ICommand> allowed = null);
    }
}
