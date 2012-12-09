using System;

namespace Veda.Interface
{
    public interface ICommandParser
    {
        IParseResult Parse(String command);
    }
}
