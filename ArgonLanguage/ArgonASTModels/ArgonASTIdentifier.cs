
namespace ArgonASTModels
{
    public class ArgonASTIdentifier : Interfaces.ValueContainer
    {
        public ArgonASTIdentifier(string varName)
        {
            VariableName = varName;
        }

        public string VariableName { get; set; }
    }
}
