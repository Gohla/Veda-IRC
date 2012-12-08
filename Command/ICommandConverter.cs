using System;

namespace Veda.Command
{
    public interface ICommandConverter : IEquatable<ICommandConverter>
    {
        Type ToType { get; }
        Type ContextType { get; }

        object Convert(String str, object context);
    }
}
