using System;
using System.Linq.Expressions;
using System.Reflection;
using Veda.Interface;

namespace Veda.Command
{
    public static class CommandBuilder
    {
        public static ICommand CreateCommand(IPlugin plugin, String name, String description, bool @private, 
            MethodInfo method, object obj = null)
        {
            return new MethodCommand(plugin, name, description, @private, method, obj);
        }

        public static ICommand CreateCommand<T>(IPlugin plugin, String name, String description, bool @private,
            Expression<T> expression)
        {
            return new ExpressionCommand<T>(plugin, name, description, @private, expression);
        }

        public static ICommandConverter CreateConverter<TFrom, TTo>(Func<TFrom, TTo> converter)
        {
            return new CommandConverterWithoutContext<TFrom, TTo>(converter);
        }

        public static ICommandConverter CreateConverter<TFrom, TTo, TContext>(Func<TFrom, TContext, TTo> converter)
        {
            return new CommandConverterWithContext<TFrom, TTo, TContext>(converter);
        }
    }
}
