using System;
using System.Linq;
using System.Linq.Expressions;
using Veda.Interface;

namespace Veda.Command
{
    public class ExpressionCommand<T> : Command
    {
        private Delegate _delegate;

        public ExpressionCommand(IPlugin plugin, String name, String description, Expression<T> expr) :
            base(plugin, name, description, 
                expr.Parameters.Select(p => p.Type).ToArray(),
                expr.Parameters.Select(p => p.Name).ToArray()
            )
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
