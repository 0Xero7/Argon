using ArgonASTModels.ValueTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTUnaryOperator : ValueContainer
    {
        public string Operator { get; set; }
        public ValueContainer left { get; set; }

        public ArgonASTUnaryOperator(string Operator, ValueContainer left)
        {
            this.Operator = Operator;
            this.left = left;
        }
    }
}
