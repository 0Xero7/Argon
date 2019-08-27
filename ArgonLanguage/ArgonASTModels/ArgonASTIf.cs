using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTIf : ArgonASTBase
    {
        public Interfaces.ValueContainer condition;
        public ArgonASTBlock trueBlock;
        public ArgonASTBlock falseBlock;
    }
}
