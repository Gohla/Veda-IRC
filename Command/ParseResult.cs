using System;
using Veda.Interface;

namespace Veda.Command
{
    public class ParseResult : IParseResult
    {
        public static readonly ParseResult Empty = new ParseResult();

        public String Name { get; set; }
        public String[] Arguments { get; set; }

        public ParseResult()
        {

        }

        public ParseResult(String commandName, params String[] arguments)
        {
            Name = commandName;
            Arguments = arguments;
        }
    }
}
