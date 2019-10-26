using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTFor : ArgonASTBase
    {
        public ArgonASTBlock init;
        public ValueTypes.ValueContainer conditional;
        public ArgonASTBlock increment;
        public ArgonASTBlock body;

        public ArgonASTFor()
        {
            this.init = new ArgonASTBlock();
            this.increment = new ArgonASTBlock();
            this.body = new ArgonASTBlock();
        }

        public ArgonASTFor(ArgonASTBlock init, ValueTypes.ValueContainer conditional, ArgonASTBlock increment)
        {
            this.init = init;
            this.conditional = conditional;
            this.increment = increment;
        }
    }
}
