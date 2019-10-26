using System;
using System.Collections.Generic;
using System.Text;

using ArgonASTModels;
using ArgonASTModels.ValueTypes;

namespace ArgonTypeResolver
{
    public static class ArgonTypeResolver
    {
        public static string GetType(ArgonASTModels.ValueTypes.ValueContainer vc)
        {
            switch (vc)
            {
                case ArgonASTIntegerLiteral _:
                    return "int";
                case ArgonASTFloatLiteral _:
                    return "float";
                case ArgonASTStringLiteral _:
                    return "string";

                case ArgonASTIdentifier id:
                    return ArgonSymbolTable.SymbolsTable.GetVariableType(id.VariableName);

                case ArgonASTUnaryOperator u:
                    return GetType(u.left);
                case ArgonASTBinaryOperator b:
                    return ArgonBinaryOperatorTypeResolver.ResolveBOPType(b);
                case ArgonASTFunctionCall f:
                    return ArgonSymbolTable.SymbolsTable.GetFunctionReturnType(f.FunctionName);
            }

            throw new NotImplementedException($"Cannot resolve type for {vc.GetType().Name} yet.");
        }
    }
}
