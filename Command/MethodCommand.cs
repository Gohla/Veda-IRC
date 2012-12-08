using System;
using System.Linq;
using System.Reflection;

namespace Veda.Command
{
    public class MethodCommand : Command
    {
        private MethodInfo _method;
        private object _obj;

        public MethodCommand(String name, String displayName, MethodInfo method, object obj):
            base(name, displayName, method.GetParameters().Select(p => p.GetType()).ToArray())
        {
            _method = method;
            _obj = obj;
        }

        public override object Call(params object[] arguments)
        {
            return _method.Invoke(_obj, arguments);
        }
    }
}
