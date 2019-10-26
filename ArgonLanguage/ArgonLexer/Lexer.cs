using System.Collections.Generic;
using ArgonSymbols;
using Models;
using TokenStream;

namespace ArgonLexer
{
    public static class Lexer
    {
        private enum LexerSymbolType
        {
            Symbol,
            Alphabet,
            Numeric,
            WhiteSpace,
            Special
        };

        private static int lineNumber = 1;

        private static List<Token> tokens;
        private static ArgonTokenStream tokenStream;

        private static void InitTokenList()
        {
            tokens = new List<Token>();
            tokenStream = new ArgonTokenStream();
            lineNumber = 1;
        }

        public static ArgonTokenStream GetTokens(string text)
        {
            InitTokenList();

            string token = "";

            LexerSymbolType type = LexerSymbolType.WhiteSpace;

            int ptr = 0;
            while (true && ptr < text.Length)
            {
                // Strings are special so handle them first
                if (text[ptr] == '\"')
                {
                    while (text[++ptr] != '\"')
                        token += text[ptr];

                    ptr++;

                    // Replace Special Characters with their LLVM equivalent
                    token = System.Text.RegularExpressions.Regex.Unescape(token);

                    AppendToTokenList(token, lineNumber, true);
                    token = "";
                    continue;
                }

                char c = text[ptr];
                type = GetSymbolType(c, type);
                token += c;

                // Increment Line Numbers
                if (c == '\n')
                    lineNumber++;

                // While another type of character is encountered keep adding to the current token
                while (++ptr < text.Length && ((GetSymbolType(text[ptr], type) == type) || (text[ptr] == '.' && type == LexerSymbolType.Numeric)))
                {
                    // Increment Line Numbers
                    if (text[ptr] == '\n')
                        lineNumber++;

                    // Enable two symbols written side by side without any whitespace to be tokenized
                    if (type == LexerSymbolType.Symbol)
                    {
                        // Is the new token also an operator?
                        if (Symbols.IsOperator(token + text[ptr]))
                            token += text[ptr];
                        else
                        {
                            AppendToTokenList(token, lineNumber);

                            token = "";
                            token += text[ptr];
                        }
                    }
                    else
                        token += text[ptr];
                }

                // Current token is complete, add it to the token list
                AppendToTokenList(token, lineNumber);
                token = "";
            }

            // If any token is remaining on the token string, add it to token list
            if (token != "")
                AppendToTokenList(token, lineNumber);

            return tokenStream;
        }

        /// <summary>
        /// Add to token list, only if token is not whitespace when it is not a string.
        /// If token is a string, add whatever it is without checking.
        /// </summary>
        private static void AppendToTokenList(string token, int lineNumber, bool isString = false)
        {
            if (isString)
            { 
                tokens.Add(new Token() { tokenValue = token, tokenType = TokenType.StringLiteral, lineNumber = lineNumber }); 
                tokenStream.AddToken(new Token() { tokenValue = token, tokenType = TokenType.StringLiteral, lineNumber = lineNumber });
                return; 
            }

            if (token.Trim() == "") return;

            Models.TokenType type = TokenType.Identifier;

            if (float.TryParse(token, out float res))
                type = TokenType.NumberLiteral;
            if (Symbols.IsOperator(token))
                type = TokenType.Operator;
            if (Symbols.IsKeyword(token))
                type = TokenType.Keyword;
            if (Symbols.IsType(token))
                type = TokenType.Type;


            tokens.Add(new Token() { tokenValue = token, tokenType = type, lineNumber = lineNumber });
            tokenStream.AddToken(new Token() { tokenValue = token, tokenType = type, lineNumber = lineNumber });
        }

        private static LexerSymbolType GetSymbolType(char arg, LexerSymbolType currentType)
        {
            if (arg == '\"') return LexerSymbolType.Special;
            if (char.IsWhiteSpace(arg)) return LexerSymbolType.WhiteSpace;
            if (char.IsLetter(arg) || arg == '_' || (char.IsNumber(arg) && currentType == LexerSymbolType.Alphabet)) return LexerSymbolType.Alphabet;
            if (char.IsNumber(arg)) return LexerSymbolType.Numeric;
            return LexerSymbolType.Symbol;
        }
    }
}