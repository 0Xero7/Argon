using System;
using System.Collections.Generic;
using System.Text;

using ArgonASTModels;

namespace ArgonASTSerializer
{
    public static class ASTSerializer
    {
        const string tab = "     ";

        public static string SerializeAST(ArgonASTBase program)
        {
            string json = "{";
            var block = program as ArgonASTModels.ArgonASTBlock;

            foreach (var child in block.Children)
                json += SerializeBlock(child, 1) + ",";

            return json + "\b }";
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
                    response = $"\"Declaration\" : {{\"Variable Name\" : \"{decl.VariableName}\",\"Type\" : \"{decl.Type}\"}}";
                    break;

                case ArgonASTAssignment ass:
                    response = $"\"Assignment\" : {{\"Variable\" : \"{ass.variable}\",\"Value\" : {{{SerializeBlock(ass.value, indent)}}}}}";
                    break;

                case ArgonASTIntegerLiteral ilit:
                    response = $"\"Literal\" : {{\"Type\" : \"int\",\"Value\" : \"{ilit.value}\"}}";
                    break;

                case ArgonASTStringLiteral slit:
                    response = $"\"Literal\" : {{\"Type\" : \"string\",\"Value\" : \"{slit.value}\"}}";
                    break;

                case ArgonASTPrint print:
                    response = $"\"Print Function\" : {{\"Expression\" : {{{SerializeBlock(print.expression, indent + 1)}}}}}";
                    break;

                case ArgonASTBinaryOperator binop:
                    response = $"\"Binary Operator\" : {{\"Operator\" : \"{binop.Operator}\",\"Left\" : {{{SerializeBlock(binop.left, indent)}}},\"Right\" : {{{SerializeBlock(binop.right, indent)}}}}}";
                    break;

                case ArgonASTIdentifier id:
                    response = $"\"Identifier\" : {{\"Name\" : \"{id.VariableName}\"}}";
                    break;

                case ArgonASTReturn ret:
                    response = $"\"Function Return\" : {{\"Expression\" : {{{SerializeBlock(ret.expression, indent)}}}}}";
                    break;

                case ArgonASTIf iff:
                    var bdy = "";
                    foreach (var x in iff.trueBlock.Children)
                        bdy += SerializeBlock(x, indent + 1) + ",";
                    bdy += "\b ";

                    var fbdy = "";
                    if (iff.falseBlock != null)
                    {
                        foreach (var x in iff.falseBlock.Children)
                            fbdy += SerializeBlock(x, indent) + ",";
                        fbdy += "\b ";
                    }

                    response = $"\"If\" : " +
                         $"{{\"Condition\" : " +
                         $"{{{SerializeBlock(iff.condition, indent)}" +
                         $"}}," +
                         $"\"True Block\" : " +
                         $"{{{bdy}}}," +
                         $"\"False Block\" : " +
                         $"{{{fbdy}}},}}";
                    break;

                case ArgonASTFunctionDeclaration fdec:
                    var body = "";
                    foreach (var x in fdec.FunctionBody.Children)
                        body += SerializeBlock(x, indent + 1) + ",";
                    body += "\b ";

                    var parameters = "";
                    foreach (var x in fdec.FormalParamaters)
                    {
                        parameters += $"{{\"Parameter Name\" : \"{x.VariableName}\",\"Type\" : \"{x.Type}\"}},";
                    }
                    if (fdec.FormalParamaters.Count > 0)
                        parameters += "\b ";


                    response = $"\"Function Declaration\" : " +
                               $"{{\"Name\" : \"{fdec.FunctionName}\"," +
                               $"\"Return type\" : \"{fdec.ReturnType}\"," +
                               $"\"Parameters\" : [{parameters}]," +
                               $"\"Body\" : " +
                               $"{{" +
                               $"{body}" +
                               $"}}" +
                               $"}}";
                    break;

                case ArgonASTFunctionCall fcall:
                    var args = "";
                    foreach (var x in fcall.parameters)
                    {
                        var ser = $"{{{SerializeBlock(x, 0)}}}";
                        args += $"{ser},";
                    }
                    if (fcall.parameters.Count > 0)
                        args += "\b ";

                    response = $"\"Function Call\" : " +
                               $"{{ \"Function Name\" : \"{fcall.FunctionName}\"," +
                               $"\"Parameters\" : [{args}]}}";
                    break;
            }

            return response;
        }
    }
}
