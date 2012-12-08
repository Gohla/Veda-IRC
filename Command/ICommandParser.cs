using System;

namespace Veda.Command
{
    public interface ICommandParser
    {
        IParseResult Parse(String command);
    }
}
