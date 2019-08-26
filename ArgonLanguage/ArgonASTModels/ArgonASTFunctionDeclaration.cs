using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTFunctionDeclaration : ArgonASTBase
    {
        public string ReturnType { get; set; }
        public string FunctionName { get; set; }
        public ArgonASTBlock FunctionBody { get; set; }
        public List<ArgonASTDeclaration> FormalParamaters { get; set; }

        public ArgonASTFunctionDeclaration(string ReturnType, string FunctionName)
        {
            this.ReturnType = ReturnType;
            this.FunctionName = FunctionName;

            FormalParamaters = new List<ArgonASTDeclaration>();
        }
    }
}
