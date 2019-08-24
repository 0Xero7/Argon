using System;
using System.Collections.Generic;
using System.Text;

using ArgonASTModels;
using ArgonASTModels.Interfaces;
using static ArgonAST.ASTBuilderUtilities;

namespace ArgonAST
{
    public static class ASTBuilder
    {
        private enum ASTState
        {
            None,
            Declaration,
            Assignment,
            Expression
        };

        private static ASTState state = ASTState.None;

        public static ArgonASTBlock GenerateAST(Span<Models.Token> arg)
        {
            int i = 0;
            int startIndex = 0;

            var block = new ArgonASTBlock();

            while (i < arg.Length)
            {
                var t = arg[i];
                
                // <type> <var-name>
                // <type> <var-name> = <expression>
                if (t.tokenType == Models.TokenType.Type)
                {
                    var decl = new ArgonASTDeclaration(t.tokenValue, arg[++i].tokenValue);
                    block.AddChild(decl);

                    // No initialization
                    if (arg[++i].IsSemicolon())
                        continue;

                    // It is an initialization
                    // Break it down into TWO entries, a decl (which has been created) and the assignment

                    // Expecting a '=', throw a helpful exception if something else is encountered
                    if (!arg[i].IsOperator("="))
                        throw new InvalidOperationException($"Unexpected symbol {arg[i]} at line {arg[i].lineNumber}. Expecting '='.");

                    startIndex = ++i;

                    // No support for multiple assignments in decls
                    // The expression is in tokens startIndex to (i - 1), after forthcoming while exits
                    while (!arg[++i].IsSemicolon());

                    // Parse the expression
                    var expr = ParseExpression(arg.Slice(startIndex, i - startIndex));

                    block.AddChild(new ArgonASTAssignment() { value = expr, variable = decl.VariableName });

                    ++i;
                }


                // <var-name> = <expression>
                if (t.tokenType == Models.TokenType.Identifier && arg[++i].IsOperator("="))
                {
                    startIndex = ++i;

                    // No support for multiple assignments in decls
                    // The expression is in tokens startIndex to (i - 1), after forthcoming while exits
                    while (!arg[++i].IsSemicolon()) ;

                    // Parse the expression
                    var expr = ParseExpression(arg.Slice(startIndex, i - startIndex));

                    block.AddChild(new ArgonASTAssignment() { value = expr, variable = t.tokenValue });

                    ++i;
                }
            }

            return block;
        }

        public static IValueContainer ParseExpression(Span<Models.Token> arg)
        {
            Stack<Models.Token> op_stack = new Stack<Models.Token>();
            Stack<IValueContainer> terminal_stack = new Stack<IValueContainer>();

            // Initialize with '(' so that no special cases exist
            op_stack.Push(new Models.Token() { tokenValue = "(", tokenType = Models.TokenType.Operator });

            for (int i = 0; i < arg.Length; i++)
            {
                switch (arg[i].tokenType)
                {
                    case Models.TokenType.Identifier:
                    case Models.TokenType.StringLiteral:
                    case Models.TokenType.NumberLiteral:
                        terminal_stack.Push(new ArgonASTIntegerLiteral(int.Parse(arg[i].tokenValue)));
                        continue;
                }

                if (arg[i].IsOperator("("))
                { op_stack.Push(arg[i]); continue; }

                Models.Token c;
                if (arg[i].IsOperator(")"))
                {
                    while (!(c = op_stack.Pop()).IsOperator("("))
                    {
                        var x = terminal_stack.Pop();
                        var y = terminal_stack.Pop();

                        terminal_stack.Push(GetASTOperator(c.tokenValue, x, y));
                    }

                    continue;
                }

                while (ArgonSymbols.Precedences.GetPrecedence(op_stack.Peek().tokenValue) >= ArgonSymbols.Precedences.GetPrecedence(arg[i].tokenValue))
                {
                    var x = terminal_stack.Pop();
                    var y = terminal_stack.Pop();

                    terminal_stack.Push(GetASTOperator(op_stack.Pop().tokenValue, x, y));
                }

                op_stack.Push(arg[i]);
            }

            Models.Token u;
            while (!(u = op_stack.Pop()).IsOperator("("))
            {
                var x = terminal_stack.Pop();
                var y = terminal_stack.Pop();

                terminal_stack.Push(GetASTOperator(u.tokenValue, x, y));
            }

            return terminal_stack.Pop();
        }

        private static ArgonASTBinaryOperator GetASTOperator(string op, IValueContainer x, IValueContainer y)
        {
            switch (op)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                    // Switch the positions of left and right. 
                    // If not done, x - y shows up at y - x
                    return new ArgonASTBinaryOperator(op, y, x);
            }

            throw new NotImplementedException($"Operator {op} is not yet supported.");
        }
    }
}
