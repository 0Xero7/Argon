
namespace ArgonASTModels
{
    public class ArgonASTIdentifier : ValueTypes.Terminal
    {
        public ArgonASTIdentifier(string varName)
        {
            VariableName = varName;
        }

        public string VariableName { get; set; }
    }
}
