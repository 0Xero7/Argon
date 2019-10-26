using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTWhile : ArgonASTBase
    {
        public ArgonASTBlock loopBlock;
        public ValueTypes.ValueContainer condition;

        public ArgonASTWhile(ValueTypes.ValueContainer condition)
        {
            this.condition = condition;
        }
    }
}
