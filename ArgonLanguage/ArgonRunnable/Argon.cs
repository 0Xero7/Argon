using System;
using System.Collections.Generic;
using Models;

namespace ArgonRunnable
{
    public static class Argon
    {
        public static List<Token> GetTokenList(string arg) => ArgonLexer.Lexer.GetTokens(arg);

        public static void GetAST(Span<Models.Token> arg)
        {
            ArgonAST.ASTBuilder.GenerateAST(arg);
        }
    }
}
