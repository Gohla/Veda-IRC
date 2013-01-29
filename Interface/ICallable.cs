using System;

namespace Veda.Interface
{
    public interface ICallable
    {
        object[] Arguments { get; }

        object Call(IContext context);
    }
}
