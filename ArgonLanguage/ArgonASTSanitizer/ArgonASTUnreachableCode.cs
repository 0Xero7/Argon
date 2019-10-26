using System;
using System.Collections.Generic;
using System.Text;

using ArgonASTModels;
using ArgonASTModels.ValueTypes;

namespace ArgonASTSanitizer
{
    public static class ArgonASTUnreachableCode
    {
        public static void RemoveUnreachableCodeFromBlock(ArgonASTBlock block)
        {
            // Protect against no false block in Ifs
            if (block == null) return;

            for (int i = 0; i < block.Children.Count; i++)
            {
                switch (block.Children[i])
                {
                    case ArgonASTBlock bl:
                        RemoveUnreachableCodeFromBlock(bl);
                        break;

                    case ArgonASTFunctionDeclaration decl:
                        RemoveUnreachableCodeFromBlock(decl.FunctionBody);
                        break;

                    case ArgonASTIf iff:
                        RemoveUnreachableCodeFromBlock(iff.trueBlock);
                        RemoveUnreachableCodeFromBlock(iff.falseBlock);
                        break;

                    case ArgonASTReturn rt:
                        block.Children.RemoveRange(i + 1, block.Children.Count - i - 1);
                        return;
                }
            }
        }
    }
}
