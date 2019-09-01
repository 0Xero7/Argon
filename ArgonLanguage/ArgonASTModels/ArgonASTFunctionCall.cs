using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTFunctionCall : ValueTypes.ValueContainer
    {
        public string FunctionName { get; set; }
        public List<ValueTypes.ValueContainer> parameters { get; set; }

        public ArgonASTFunctionCall(string FunctionName)
        {
            this.FunctionName = FunctionName;
            parameters = new List<ValueTypes.ValueContainer>(); 
        }
    }
}
