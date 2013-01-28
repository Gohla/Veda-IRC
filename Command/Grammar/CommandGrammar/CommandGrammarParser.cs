/*
 * CommandGrammarParser.cs
 *
 * THIS FILE HAS BEEN GENERATED AUTOMATICALLY. DO NOT EDIT!
 */

using System.IO;

using PerCederberg.Grammatica.Runtime;

namespace Veda.Command.Grammar {

    /**
     * <remarks>A token stream parser.</remarks>
     */
    internal class CommandGrammarParser : RecursiveDescentParser {

        /**
         * <summary>An enumeration with the generated production node
         * identity constants.</summary>
         */
        private enum SynteticPatterns {
            SUBPRODUCTION_1 = 3001
        }

        /**
         * <summary>Creates a new parser with a default analyzer.</summary>
         *
         * <param name='input'>the input stream to read from</param>
         *
         * <exception cref='ParserCreationException'>if the parser
         * couldn't be initialized correctly</exception>
         */
        public CommandGrammarParser(TextReader input)
            : base(input) {

            CreatePatterns();
        }

        /**
         * <summary>Creates a new parser.</summary>
         *
         * <param name='input'>the input stream to read from</param>
         *
         * <param name='analyzer'>the analyzer to parse with</param>
         *
         * <exception cref='ParserCreationException'>if the parser
         * couldn't be initialized correctly</exception>
         */
        public CommandGrammarParser(TextReader input, CommandGrammarAnalyzer analyzer)
            : base(input, analyzer) {

            CreatePatterns();
        }

        /**
         * <summary>Creates a new tokenizer for this parser. Can be overridden
         * by a subclass to provide a custom implementation.</summary>
         *
         * <param name='input'>the input stream to read from</param>
         *
         * <returns>the tokenizer created</returns>
         *
         * <exception cref='ParserCreationException'>if the tokenizer
         * couldn't be initialized correctly</exception>
         */
        protected override Tokenizer NewTokenizer(TextReader input) {
            return new CommandGrammarTokenizer(input);
        }

        /**
         * <summary>Initializes the parser by creating all the production
         * patterns.</summary>
         *
         * <exception cref='ParserCreationException'>if the parser
         * couldn't be initialized correctly</exception>
         */
        private void CreatePatterns() {
            ProductionPattern             pattern;
            ProductionPatternAlternative  alt;

            pattern = new ProductionPattern((int) CommandGrammarConstants.COMMANDS,
                                            "Commands");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) CommandGrammarConstants.COMMAND, 1, 1);
            alt.AddProduction((int) SynteticPatterns.SUBPRODUCTION_1, 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) CommandGrammarConstants.COMMAND,
                                            "Command");
            alt = new ProductionPatternAlternative();
            alt.AddProduction((int) CommandGrammarConstants.EXPRESSION, 1, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) CommandGrammarConstants.EXPRESSION,
                                            "Expression");
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) CommandGrammarConstants.TEXT, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) CommandGrammarConstants.STRING, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) CommandGrammarConstants.PARAMETER, 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) CommandGrammarConstants.COMMAND_START, 1, 1);
            alt.AddProduction((int) CommandGrammarConstants.COMMAND, 1, 1);
            alt.AddToken((int) CommandGrammarConstants.COMMAND_END, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern((int) SynteticPatterns.SUBPRODUCTION_1,
                                            "Subproduction1");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken((int) CommandGrammarConstants.COMMAND_SEPARATOR, 1, 1);
            alt.AddProduction((int) CommandGrammarConstants.COMMAND, 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);
        }
    }
}
