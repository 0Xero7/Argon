using ArgonASTModels.ValueTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTAssignment : ArgonASTBase
    {
        public ValueContainer value;
        public string variable;

        public ArgonASTAssignment() { }

        public ArgonASTAssignment(string variable, ValueContainer value) 
        {
            this.variable = variable;
            this.value = value;
        }
    }
}
