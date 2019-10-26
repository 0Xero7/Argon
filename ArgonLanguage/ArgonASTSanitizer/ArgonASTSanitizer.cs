using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTSanitizer
{
    // Sanitizes the generated AST for the Code-Generator
    public static class ArgonASTSanitizer
    {
        public static void SantizeAST(ArgonASTModels.ArgonASTBlock block)
        {
            ArgonASTUnreachableCode.RemoveUnreachableCodeFromBlock(block);
        }
    }
}
