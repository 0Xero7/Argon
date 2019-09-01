using ArgonASTModels.ValueTypes;

namespace ArgonASTModels
{
    public class ArgonASTBinaryOperator : ValueContainer
    {
        public string Operator { get; set; }
        public ValueContainer left { get; set; }
        public ValueContainer right { get; set; }

        public ArgonASTBinaryOperator(string Operator, ValueContainer left, ValueContainer right)
        {
            this.Operator = Operator;
            this.left = left;
            this.right = right;
        }
    }
}
