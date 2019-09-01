using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTReturn : ArgonASTBase
    {
        public ValueTypes.ValueContainer expression;
        public ArgonASTReturn(ValueTypes.ValueContainer expression)
        {
            this.expression = expression;
        }
    }
}
