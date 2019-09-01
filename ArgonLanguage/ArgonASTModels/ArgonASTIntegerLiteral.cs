namespace ArgonASTModels
{
    public class ArgonASTIntegerLiteral : ValueTypes.Terminal
    {
        public int value { get; set; }

        public ArgonASTIntegerLiteral(int value)
        {
            this.value = value;
        }
    }
}
