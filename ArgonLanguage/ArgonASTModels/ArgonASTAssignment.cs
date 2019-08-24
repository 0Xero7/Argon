using ArgonASTModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTAssignment : ArgonASTBase
    {
        public IValueContainer value;
        public string variable;
    }
}
