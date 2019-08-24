using ArgonASTModels;
using ArgonASTModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonAST
{
    public class ArgonASTBinaryOperator : ArgonASTBase, IValueContainer
    {
        public string Operator { get; set; }
        public IValueContainer left { get; set; }
        public IValueContainer right { get; set; }

        public ArgonASTBinaryOperator(string Operator, IValueContainer left, IValueContainer right)
        {
            this.Operator = Operator;
            this.left = left;
            this.right = right;
        }
    }
}
