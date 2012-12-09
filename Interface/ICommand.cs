using System;

namespace Veda.Interface
{
    public interface ICommand : IEquatable<ICommand>
    {
        IPlugin Plugin { get; }
        String Name { get; }
        String Description { get; }
        Type[] ParameterTypes { get; }

        bool IsCompatible(params Type[] argumentTypes);
        bool IsPartialCompatible(params Type[] argumentTypes);
        object Call(params object[] arguments);
    }
}