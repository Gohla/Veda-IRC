using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using PerCederberg.Grammatica.Runtime;
using Veda.Command.Grammar;
using Veda.Interface;

namespace Veda.Command
{
    public class CommandsExpression : IExpression
    {
        public List<CommandExpression> Commands = new List<CommandExpression>();

        public ushort Arity { get; set; }

        public IObservable<object> Evaluate(IContext context, object[] arguments)
        {
            return Commands
                .Select(command => context.Evaluate(command.Evaluate(context, arguments)))
                .Concat()
                ;
        }
    }

    public interface ICommandExpression
    {
        IObservable<object> Evaluate(IContext context, object[] arguments);
    }

    public class CommandExpression : ICommandExpression
    {
        public List<ICommandExpression> Expressions = new List<ICommandExpression>();

        public IObservable<object> Evaluate(IContext context, object[] arguments)
        {
            var results = Expressions
                .Select(e => context.Evaluate(e.Evaluate(context, arguments)).Wait()) // TODO: Wait() is blocking!
                .ToArray()
                ;

            try
            {
                ICallable callable = context.Bot.Command.CallParsed(context.ConversionContext, results);
                return context.Evaluate(callable);
            }
            catch(Exception e)
            {
                return Observable.Throw<String>(e);
            }
        }
    }

    public class Literal : ICommandExpression
    {
        public String Text;

        public IObservable<object> Evaluate(IContext context, object[] arguments)
        {
            return Observable.Return(Text);
        }
    }

    public class Parameter : ICommandExpression
    {
        public ushort Index;

        public IObservable<object> Evaluate(IContext context, object[] arguments)
        {
            if(Index > arguments.Length)
                return Observable.Return("$" + Index);
            return Observable.Return(arguments[Index - 1]);
        }
    }

    internal class ParseAnalyzer : CommandGrammarAnalyzer
    {
        private Stack<CommandExpression> _commandStack = new Stack<CommandExpression>();

        public CommandsExpression Commands { get; private set; }

        public ParseAnalyzer()
        {
            Commands = new CommandsExpression();
        }

        public override void Reset()
        {
            base.Reset();

            _commandStack.Clear();
            Commands = new CommandsExpression();
        }

        public override void EnterCommand(Production node)
        {
            _commandStack.Push(new CommandExpression());
        }

        public override Node ExitCommand(Production node)
        {
            CommandExpression command = _commandStack.Pop();
            if(_commandStack.Count == 0)
                Commands.Commands.Add(command);
            else
                _commandStack.Peek().Expressions.Add(command);

            return node;
        }

        public override void EnterParameter(Token node)
        {
            _commandStack.Peek().Expressions.Add(new Parameter { Index = UInt16.Parse(node.Image.Substring(1)) });
            ++Commands.Arity;
        }

        public override void EnterString(Token node)
        {
            _commandStack.Peek().Expressions.Add(new Literal { Text = node.Image.Substring(1, node.Image.Length - 2) });
        }

        public override void EnterText(Token node)
        {
            _commandStack.Peek().Expressions.Add(new Literal { Text = node.Image });
        }
    }

    public class CommandParser : ICommandParser
    {
        private readonly CommandGrammarParser _parser;
        private readonly ParseAnalyzer _analyzer = new ParseAnalyzer();

        public CommandParser()
        {
            _parser = new CommandGrammarParser(new StringReader(String.Empty), _analyzer);
            _parser.Prepare();
        }

        public IExpression Parse(String str)
        {
            if(String.IsNullOrWhiteSpace(str))
                return null;
            _parser.Reset(new StringReader(str));
            _parser.Parse();
            if(_analyzer.Commands.Commands.Count == 0)
                return null;
            return _analyzer.Commands;
        }
    }
}
