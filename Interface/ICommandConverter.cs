using System;

namespace Veda.Interface
{
    public interface ICommandConverter : IEquatable<ICommandConverter>
    {
        Type FromType { get; }
        Type ToType { get; }
        Type ContextType { get; }

        object Convert(object obj, object context);
    }
}
