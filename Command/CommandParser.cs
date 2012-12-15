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
        private class ParseAnalyzer : CommandGrammarAnalyzer
        {
            public List<String> Arguments = new List<String>();

            public override void EnterText(Token node)
            {
                Arguments.Add(node.Image);
            }

            public override void EnterString(Token node)
            {
                Arguments.Add(node.Image.Substring(1, node.Image.Length - 2));
            }
        }
        
        private readonly CommandGrammarParser _parser;
        private readonly ParseAnalyzer _analyzer = new ParseAnalyzer();
        private char _command = '~';

        public CommandParser()
        {
            _parser = new CommandGrammarParser(new StringReader(String.Empty), _analyzer);
            _parser.Prepare();
        }

        public String[] Parse(String str)
        {
            if(String.IsNullOrWhiteSpace(str) || str.Length < 2 || str[0] != _command)
                return null;
            _parser.Reset(new StringReader(str.Substring(1)));
            _analyzer.Arguments.Clear();
            Node command = _parser.Parse();
            if(_analyzer.Arguments.IsEmpty())
                return null;
            return _analyzer.Arguments.ToArray();
        }
    }
}
