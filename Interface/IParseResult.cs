using System;

namespace Veda.Interface
{
    public interface IParseResult
    {
        String Name { get; }
        String[] Arguments { get; }
    }
}
