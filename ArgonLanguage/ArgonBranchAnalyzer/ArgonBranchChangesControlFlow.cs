using ArgonASTModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonBranchAnalyzer
{
    public static class ArgonBranchChangesControlFlow
    {
        public static bool DoesBranchChangeControlFlow(ArgonASTModels.ArgonASTBlock block)
        {
            // If block is null or empty, it doesn't change control flow
            if (block == null || block.Children == null || block.Children.Count == 0)
                return false;

            if (ArgonBranchAnalyzer.ArgonAnalyzerBranchReturn.DoesBranchReturnFromAllPaths(block))
                return true;

            foreach (var b in block.Children)
            {
                switch (b)
                {
                    case ArgonASTIf iff:
                        return DoesBranchChangeControlFlow(iff.trueBlock) && DoesBranchChangeControlFlow(iff.falseBlock);

                    case ArgonASTWhile wh:
                        return DoesBranchChangeControlFlow(wh.loopBlock);


                    case ArgonASTContinue _:
                        return true;

                    case ArgonASTReturn ret:
                        return true;
                }
            }

            return false;
        }
    }
}
