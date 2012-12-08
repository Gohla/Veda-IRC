using System;
using System.Linq;
using System.Linq.Expressions;

namespace Veda.Command
{
    public class ExpressionCommand<T> : Command
    {
        private Delegate _delegate;

        public ExpressionCommand(String name, String description, Expression<T> expr) :
            base(name, description, expr.Parameters.Select(p => p.Type).ToArray())
        {
            LambdaExpression lambdaExpr = expr;
            _delegate = lambdaExpr.Compile();
        }

        public override object Call(params object[] arguments)
        {
            return _delegate.DynamicInvoke(arguments);
        }
    }
}
