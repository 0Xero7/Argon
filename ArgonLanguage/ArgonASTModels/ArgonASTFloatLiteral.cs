using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTFloatLiteral : ValueTypes.Terminal
    {
        public float value { get; set; }

        public ArgonASTFloatLiteral(float value)
        {
            this.value = value;
        }
    }
}
