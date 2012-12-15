using System;

namespace Veda.Interface
{
    public interface ICommand : IEquatable<ICommand>
    {
        IPlugin Plugin { get; }
        String Name { get; }
        String Description { get; }
        Type[] ParameterTypes { get; }
        String[] ParameterNames { get; }

        bool IsCompatible(params Type[] argumentTypes);
        bool IsPartialCompatible(params Type[] argumentTypes);
        object Call(IContext context, params object[] arguments);
    }
}