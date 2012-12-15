using System;
using System.Collections.Generic;
using System.IO;
using PerCederberg.Grammatica.Runtime;
using Veda.Command.Grammar;
using Veda.Interface;

namespace Veda.Command
{
    public class CommandParser : ICommandParser
    {
        private readonly CommandGrammarParser _parser;

        public CommandParser()
        {
            _parser = new CommandGrammarParser(new StringReader(String.Empty));
            _parser.Prepare();
        }

        public String[] Parse(String str)
        {
            _parser.Reset(new StringReader(str));
            Node command = _parser.Parse();

            if(command.Id == (int)CommandGrammarConstants.COMMAND)
            {
                List<String> args = new List<String>();
                for(int i = 0; i < command.GetChildCount(); ++i)
                {
                    Node argument = command.GetChildAt(i);
                    if(argument.Id == (int)CommandGrammarConstants.ARGUMENT)
                    {
                        Token contents = argument.GetChildAt(0) as Token;
                        switch(contents.Id)
                        {
                            case (int)CommandGrammarConstants.TEXT:
                                args.Add(contents.Image);
                                break;
                            case (int)CommandGrammarConstants.STRING:
                                args.Add(contents.Image.Substring(1, contents.Image.Length - 2));
                                break;
                        }
                        
                    }
                }
                return args.ToArray();
            }
            return null;
        }
    }
}
