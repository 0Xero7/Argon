namespace ArgonASTModels
{
    public class ArgonASTIntegerLiteral : Interfaces.ValueContainer
    {
        public int value { get; set; }

        public ArgonASTIntegerLiteral(int value)
        {
            this.value = value;
        }
    }
}
