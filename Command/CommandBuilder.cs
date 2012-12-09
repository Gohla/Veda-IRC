using System;
using System.Linq.Expressions;
using System.Reflection;
using Veda.Interface;

namespace Veda.Command
{
    public static class CommandBuilder
    {
        public static ICommand CreateCommand(String name, String description, MethodInfo method, object obj = null)
        {
            return new MethodCommand(name, description, method, obj);
        }

        public static ICommand CreateCommand<T>(String name, String description, Expression<T> expression)
        {
            return new ExpressionCommand<T>(name, description, expression);
        }

        public static ICommandConverter CreateConverter<TTo>(Func<String, TTo> converter)
        {
            return new CommandConverterWithoutContext<TTo>(converter);
        }

        public static ICommandConverter CreateConverter<TTo, TContext>(Func<String, TContext, TTo> converter)
        {
            return new CommandConverterWithContext<TTo, TContext>(converter);
        }
    }
}
