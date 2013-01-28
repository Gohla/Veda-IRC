using System;

namespace Veda.Interface
{
    public interface IExpression
    {
        ushort Arity { get; }

        IObservable<object> Evaluate(IContext context, object[] arguments, Action<ICommand> allowed = null);
    }
}
