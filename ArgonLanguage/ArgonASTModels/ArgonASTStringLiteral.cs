namespace ArgonASTModels
{
    public class ArgonASTStringLiteral : ArgonASTModels.Interfaces.ValueContainer
    {
        public string value { get; set; }

        public ArgonASTStringLiteral(string value)
        {
            this.value = value;
        }
    }
}
