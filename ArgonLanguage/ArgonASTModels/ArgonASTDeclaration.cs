using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTDeclaration : ArgonASTBase
    {
        public string Type { get; set; }

        public string VariableName { get; set; }

        public ArgonASTDeclaration(string Type, string VariableName)
        {
            this.Type = Type;
            this.VariableName = VariableName;
        }
    }
}
