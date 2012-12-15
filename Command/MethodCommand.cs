using System;
using System.Linq;
using System.Reflection;
using Veda.Interface;

namespace Veda.Command
{
    public class MethodCommand : AbstractCommand
    {
        private MethodInfo _method;
        private object _obj;

        public MethodCommand(IPlugin plugin, String name, String description, MethodInfo method, object obj) :
            base(plugin, name, description, 
                method.GetParameters().Skip(1).Select(p => p.ParameterType).ToArray(),
                method.GetParameters().Skip(1).Select(p => p.Name).ToArray()
            )
        {
            _method = method;
            _obj = obj;
        }

        public override object Call(IContext context, params object[] arguments)
        {
            try
            {
                // TODO: More efficient array concat?
                return _method.Invoke(_obj, context.AsEnumerable().Concat(arguments).ToArray());
            }
            catch(TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }
}
