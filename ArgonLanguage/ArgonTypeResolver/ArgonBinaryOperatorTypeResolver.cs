using System;
using System.Collections.Generic;
using System.Text;

using ArgonASTModels;
using ArgonASTModels.ValueTypes;

namespace ArgonTypeResolver
{
    public static class ArgonBinaryOperatorTypeResolver
    {
        private static Dictionary<(string, string), string> BOPReturns = new Dictionary<(string, string), string>()
        {
            {("int", "int"),        "int" },
            {("float", "int"),      "float" },
            {("int", "float"),      "float" },
            {("float", "float"),    "float" },
            {("string", "string"),  "string" }
        };            

        public static string ResolveBOPType(ArgonASTBinaryOperator b)
        {
            string leftType = ArgonTypeResolver.GetType(b.left);
            string rightType = ArgonTypeResolver.GetType(b.right);

            return BOPReturns[(leftType, rightType)];
        }
    }
}
