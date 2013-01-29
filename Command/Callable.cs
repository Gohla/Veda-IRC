using System;
using Veda.Interface;

namespace Veda.Command
{
    public class CommandCallable : ICallable
    {
        private ICommand _command;

        public object[] Arguments { get; private set; }

        public CommandCallable(ICommand command, object[] arguments)
        {
            _command = command;
            Arguments = arguments;
        }

        public object Call(IContext context)
        {
            if(context.CallDepth > 10)
                throw new InvalidOperationException("Command recursing too deep, execution halted.");

            if(context.Allowed != null)
                context.Allowed(_command);

            ++context.CallDepth;
            ICommand prevCommand = context.Command;
            context.Command = _command;
            object obj = _command.Call(context, Arguments);
            context.Command = prevCommand;
            return obj;
        }
    }

    public class ExpressionCallable : ICallable
    {
        private IExpression _expression;

        public object[] Arguments { get; private set; }

        public ExpressionCallable(IExpression expression, object[] arguments)
        {
            _expression = expression;
            Arguments = arguments;
        }

        public object Call(IContext context)
        {
            if(context.CallDepth > 10)
                throw new InvalidOperationException("Command recursing too deep, execution halted.");

            ++context.CallDepth;
            return _expression.Evaluate(context, Arguments);
        }
    }
}
