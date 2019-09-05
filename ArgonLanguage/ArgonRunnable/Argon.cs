using System;
using System.Collections.Generic;
using Models;

namespace ArgonRunnable
{
    public static class Argon
    {
        public static List<Token> GetTokenList(string arg) => ArgonLexer.Lexer.GetTokens(arg);

        public static ArgonASTModels.ArgonASTBase GetAST(Span<Models.Token> arg)
        {
            return ArgonAST.ASTBuilder.GenerateAST(arg);
        }

        public static void SanitizeAST(ArgonASTModels.ArgonASTBase arg)
        {
            ArgonASTSanitizer.ArgonASTSanitizer.SantizeAST(arg as ArgonASTModels.ArgonASTBlock);
        }

        public static void PrintAST(ArgonASTModels.ArgonASTBase prog)
        {
            Console.WriteLine(ArgonASTSerializer.ASTSerializer.SerializeAST(prog));
        }

        public static void GenerateIR(ArgonASTModels.ArgonASTBase prog)
        {
            ArgonCodeGen.ArgonCodeGen.GetGeneratedCode(prog);
        }
    }
}
