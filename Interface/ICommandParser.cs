using System;

namespace Veda.Interface
{
    public interface ICommandParser
    {
        String[] Parse(String command);
    }
}
