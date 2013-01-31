/*
 * CommandGrammarTokenizer.cs
 *
 * THIS FILE HAS BEEN GENERATED AUTOMATICALLY. DO NOT EDIT!
 */

using System.IO;

using PerCederberg.Grammatica.Runtime;

namespace Veda.Command.Grammar {

    /**
     * <remarks>A character stream tokenizer.</remarks>
     */
    internal class CommandGrammarTokenizer : Tokenizer {

        /**
         * <summary>Creates a new tokenizer for the specified input
         * stream.</summary>
         *
         * <param name='input'>the input stream to read</param>
         *
         * <exception cref='ParserCreationException'>if the tokenizer
         * couldn't be initialized correctly</exception>
         */
        public CommandGrammarTokenizer(TextReader input)
            : base(input, false) {

            CreatePatterns();
        }

        /**
         * <summary>Initializes the tokenizer by creating all the token
         * patterns.</summary>
         *
         * <exception cref='ParserCreationException'>if the tokenizer
         * couldn't be initialized correctly</exception>
         */
        private void CreatePatterns() {
            TokenPattern  pattern;

            pattern = new TokenPattern((int) CommandGrammarConstants.STRING,
                                       "STRING",
                                       TokenPattern.PatternType.REGEXP,
                                       "\"([^\"\\\\;\\[\\]]|\"\"|\\\\.)*\"");
            AddPattern(pattern);

            pattern = new TokenPattern((int) CommandGrammarConstants.TEXT,
                                       "TEXT",
                                       TokenPattern.PatternType.REGEXP,
                                       "[^ \\t\\n\\r\"$;\\[\\]]+");
            AddPattern(pattern);

            pattern = new TokenPattern((int) CommandGrammarConstants.PARAMETER,
                                       "PARAMETER",
                                       TokenPattern.PatternType.REGEXP,
                                       "\\$[1-9]");
            AddPattern(pattern);

            pattern = new TokenPattern((int) CommandGrammarConstants.COMMAND_START,
                                       "COMMAND_START",
                                       TokenPattern.PatternType.STRING,
                                       "[");
            AddPattern(pattern);

            pattern = new TokenPattern((int) CommandGrammarConstants.COMMAND_END,
                                       "COMMAND_END",
                                       TokenPattern.PatternType.STRING,
                                       "]");
            AddPattern(pattern);

            pattern = new TokenPattern((int) CommandGrammarConstants.COMMAND_SEPARATOR,
                                       "COMMAND_SEPARATOR",
                                       TokenPattern.PatternType.STRING,
                                       ";");
            AddPattern(pattern);

            pattern = new TokenPattern((int) CommandGrammarConstants.LAYOUT,
                                       "LAYOUT",
                                       TokenPattern.PatternType.REGEXP,
                                       "[ \\t\\n\\r]+");
            pattern.Ignore = true;
            AddPattern(pattern);
        }
    }
}
