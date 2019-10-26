using System;
using System.Collections.Generic;
using System.Text;

using ArgonASTModels;

namespace ArgonBranchAnalyzer
{
    public static class ArgonAnalyzerBranchReturn
    {
        public static bool DoesBranchReturnFromAllPaths(ArgonASTBlock block)
        {
            // If block is null or empty, it doesn't return anything
            if (block == null || block.Children == null || block.Children.Count == 0)
                return false;

            // If the last instruction is a return, then the block definitely returns something
            if (block.Children[block.Children.Count - 1] is ArgonASTReturn)
                return true;

            foreach (var b in block.Children)
            {
                switch (b)
                {
                    case ArgonASTIf iff:
                        return DoesBranchReturnFromAllPaths(iff.trueBlock) && DoesBranchReturnFromAllPaths(iff.falseBlock);

                    case ArgonASTWhile wh:
                        return DoesBranchReturnFromAllPaths(wh.loopBlock);

                    case ArgonASTReturn ret:
                        return true;
                }
            }

            return false;
        }
    }
}
