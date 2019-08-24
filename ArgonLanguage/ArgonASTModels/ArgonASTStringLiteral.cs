using ArgonASTModels;

namespace ArgonAST
{
    public class ArgonASTStringLiteral : ArgonASTBase
    {
        public string value { get; set; }

        public ArgonASTStringLiteral(string value)
        {
            this.value = value;
        }
    }
}
