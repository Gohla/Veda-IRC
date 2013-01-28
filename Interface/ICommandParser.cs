using System;

namespace Veda.Interface
{
    public interface ICommandParser
    {
        IExpression Parse(String command);
    }
}
