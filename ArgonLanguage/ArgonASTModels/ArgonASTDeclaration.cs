using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTDeclaration : ArgonASTBase
    {
        public string Type { get; set; }
        public int ptrDepth { get; set; }
        public string VariableName { get; set; }

        public ArgonASTDeclaration(string Type, string VariableName, int ptrDepth = 0)
        {
            this.Type = Type;
            this.VariableName = VariableName;
            this.ptrDepth = ptrDepth;
        }
    }
}
