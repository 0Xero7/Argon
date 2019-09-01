namespace ArgonASTModels
{
    public class ArgonASTStringLiteral : ValueTypes.Terminal
    {
        public string value { get; set; }

        public ArgonASTStringLiteral(string value)
        {
            this.value = value;
        }
    }
}
