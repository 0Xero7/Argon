using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTFunctionCall : Interfaces.ValueContainer
    {
        public string FunctionName { get; set; }
        public List<Interfaces.ValueContainer> parameters { get; set; }

        public ArgonASTFunctionCall(string FunctionName)
        {
            this.FunctionName = FunctionName;
            parameters = new List<Interfaces.ValueContainer>(); 
        }
    }
}
