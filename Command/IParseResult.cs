using System;

namespace Veda.Command
{
    public interface IParseResult
    {
        String Name { get; }
        String[] Arguments { get; }
    }
}
