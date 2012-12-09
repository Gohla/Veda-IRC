using System;
using System.Collections.Generic;
using System.Text;
using Veda.Interface;

namespace Veda.Command
{
    public class CommandParser : ICommandParser
    {
        private char _start = '~';
        private char _separator = ' ';
        private char _joiner = '"';
        private char _escape = '\\';

        public CommandParser()
        {

        }

        public String[] Parse(String command)
        {
            String str = command.Trim();
            if(str.Length < 2)
                return null;

            if(str[0] != _start)
                return null;

            bool joiner = false;
            StringBuilder builder = new StringBuilder();

            List<String> args = new List<String>();
            for(int i = 1; i < str.Length; ++i)
            {
                if(str[i] == _escape)
                {
                    if(++i != str.Length)
                        builder.Append(str[i]);
                    else
                        break;
                }
                else if(str[i] == _joiner && !joiner)
                {
                    joiner = true;
                }
                else if((str[i] == _separator && !joiner) || (str[i] == _joiner && joiner))
                {
                    args.Add(builder.ToString());
                    builder.Clear();
                }
                else
                {
                    builder.Append(str[i]);
                }
            }

            if(builder.Length > 0)
            {
                args.Add(builder.ToString());
            }

            return args.ToArray();
        }
    }
}
