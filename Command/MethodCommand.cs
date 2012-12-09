using System;
using System.Linq;
using System.Reflection;

namespace Veda.Command
{
    public class MethodCommand : Command
    {
        private MethodInfo _method;
        private object _obj;

        public MethodCommand(String name, String description, MethodInfo method, object obj):
            base(name, description, method.GetParameters().Select(p => p.ParameterType).ToArray())
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
