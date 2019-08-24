namespace ArgonASTModels
{
    public class ArgonASTIntegerLiteral : Interfaces.IValueContainer
    {
        public int value { get; set; }

        public ArgonASTIntegerLiteral(int value)
        {
            this.value = value;
        }
    }
}
