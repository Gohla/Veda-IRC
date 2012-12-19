using System;
using System.Linq;
using System.Linq.Expressions;
using Veda.Interface;

namespace Veda.Command
{
    public class ExpressionCommand<T> : AbstractCommand
    {
        private Delegate _delegate;

        public ExpressionCommand(IPlugin plugin, String name, String description, bool @private, Expression<T> expr) :
            base(plugin, name)
        {
            Description = description;
            ParameterTypes = expr.Parameters.Select(p => p.Type).ToArray();
            ParameterNames = expr.Parameters.Select(p => p.Name).ToArray();
            Private = @private;

            LambdaExpression lambdaExpr = expr;
            _delegate = lambdaExpr.Compile();
        }

        public override object Call(IContext context, params object[] arguments)
        {
            // TODO: More efficient array concat?
            return _delegate.DynamicInvoke(context.AsEnumerable().Concat(arguments).ToArray());
        }
    }
}
