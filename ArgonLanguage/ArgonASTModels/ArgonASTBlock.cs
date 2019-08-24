using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTModels
{
    public class ArgonASTBlock
    {
        public List<ArgonASTBase> Children;

        public ArgonASTBlock()
        {
            Children = new List<ArgonASTBase>();
        }

        public void AddChild(ArgonASTBase arg)
        { Children.Add(arg); }
    }
}
