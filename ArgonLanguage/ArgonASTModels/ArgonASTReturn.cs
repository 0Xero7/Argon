using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTReturn : ArgonASTBase
    {
        public Interfaces.ValueContainer expression;
        public ArgonASTReturn(Interfaces.ValueContainer expression)
        {
            this.expression = expression;
        }
    }
}
