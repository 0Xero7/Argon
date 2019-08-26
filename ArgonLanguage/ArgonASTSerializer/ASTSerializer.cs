using System;
using System.Collections.Generic;
using System.Text;

using ArgonASTModels;

namespace ArgonASTSerializer
{
    public static class ASTSerializer
    {
        public static string SerializeAST(ArgonASTBase program)
        {
            string json = "Program: \n{";
            var block = program as ArgonASTModels.ArgonASTBlock;

            foreach (var child in block.Children)
                json += SerializeBlock(child, 1) + ",";

            return json + "\b \n}";
        }

        private static string SerializeBlock(ArgonASTBase arg, int indent)
        {
            var wp = "\t";
            for (int i = 1; i < indent; i++)
                wp += "\t";

            string response = "";

            switch (arg)
            {
                case ArgonASTDeclaration decl:
                    response = $"\nDeclaration : \n{{\n\tVariable Name : '{decl.VariableName}',\n\tType : '{decl.Type}'\n}}";
                    break;
                case ArgonASTAssignment ass:
                    response = $"\nAssignment : \n{{\n\tVariable : '{ass.variable}',\n\tValue : \n\t{{{SerializeBlock(ass.value, indent + 1)}\n\t}}\n}}";
                    break;
                case ArgonASTIntegerLiteral ilit:
                    response = $"\nLiteral : \n{{\n\tType : 'int',\n\tValue : '{ilit.value}'\n}}";
                    break;
                case ArgonASTStringLiteral slit:
                    response = $"\nLiteral : \n{{\n\tType : 'string',\n\tValue : '{slit.value}'\n}}";
                    break;
                case ArgonASTPrint print:
                    response = $"\nPrint Function : \n{{\n\tExpression : \n\t{{\n\t{SerializeBlock(print.expression, indent + 1)}\n\t}}\n}}";
                    break;
                case ArgonASTBinaryOperator binop:
                    response = $"\nBinary Operator : \n{{\n\tOperator : '{binop.Operator}',\n\tLeft : \n\t{{\n{SerializeBlock(binop.left, indent)}\n\t}},\n\tRight : \n\t{{\n{SerializeBlock(binop.right, indent)}\n\t}}\n}}";
                    break;
                case ArgonASTIdentifier id:
                    response = $"\nIdentifier : \n{{\n\tName : '{id.VariableName}'\n}}";
                    break;
                case ArgonASTFunctionDeclaration fdec:
                    response = $"\nFunction Declaration : \n{{\n\tName : '{fdec.FunctionName}'\n}}";
                    break;
            }
            
            return response.Replace("\n", "\n"+wp);
        }
    }
}
