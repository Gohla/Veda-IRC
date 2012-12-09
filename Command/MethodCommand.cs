using System;
using System.Linq;
using System.Reflection;
using Veda.Interface;

namespace Veda.Command
{
    public class MethodCommand : Command
    {
        private MethodInfo _method;
        private object _obj;

        public MethodCommand(IPlugin plugin, String name, String description, MethodInfo method, object obj) :
            base(plugin, name, description, 
                method.GetParameters().Select(p => p.ParameterType).ToArray(),
                method.GetParameters().Select(p => p.Name).ToArray()
            )
        {
            _method = method;
            _obj = obj;
        }

        public override object Call(params object[] arguments)
        {
            try
            {
                return _method.Invoke(_obj, arguments);
            }
            catch(TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }
}
