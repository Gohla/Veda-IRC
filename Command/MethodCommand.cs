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

        public MethodCommand(IPlugin plugin, String name, String description, bool @private, MethodInfo method, 
            object obj) :
            base(plugin, name)
        {
            Description = description;
            ParameterTypes = method.GetParameters().Skip(1).Select(p => p.ParameterType).ToArray();
            ParameterNames = method.GetParameters().Skip(1).Select(p => p.Name).ToArray();
            Private = @private;

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
