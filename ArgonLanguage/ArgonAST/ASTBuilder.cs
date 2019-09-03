using System;
using System.Collections.Generic;
using System.Text;

using ArgonASTModels;
using ArgonASTModels.ValueTypes;
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

                // Function Decl 
                // <ret-type> <func-name>(<type1> <var-name1>, <type2> <var-name2>,...) { ... }
                if (t.tokenType == Models.TokenType.Type && arg[i + 1].tokenType == Models.TokenType.Identifier && arg[i + 2].IsOperator("("))
                {
                    var decl = new ArgonASTFunctionDeclaration(t.tokenValue, arg[++i].tokenValue);

                    i += 1;

                    // Read the formal parameters
                    while (!arg[++i].IsOperator(")"))
                    {
                        var type = arg[i].tokenValue;
                        var name = arg[++i].tokenValue;

                        if (arg[i + 1].IsOperator(","))
                            ++i;

                        var param = new ArgonASTDeclaration(type, name);

                        decl.FormalParamaters.Add(param);
                    }

                    // Capture and recursively parse the body of the function

                    // No function body, throw exception
                    if (!arg[++i].IsOperator("{"))
                        throw new InvalidProgramException($"Function body doesn't have a '{{'. Found {arg[i].tokenValue}.");

                    startIndex = ++i;
                    int depth = 1, endIndex = startIndex;

                    while (depth > 0)
                    {
                        if (arg[i + 1].IsOperator("{")) depth++;
                        if (arg[i + 1].IsOperator("}")) depth--;

                        ++i;

                        endIndex++;
                    }

                    decl.FunctionBody = GenerateAST(arg.Slice(startIndex, endIndex - startIndex));

                    ++i;

                    block.AddChild(decl);

                    continue;
                }

                // Variable Decl 
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
                    while (!arg[++i].IsSemicolon()) ;

                    // Parse the expression
                    var expr = ParseExpression(arg.Slice(startIndex, i - startIndex));

                    block.AddChild(new ArgonASTAssignment() { value = expr, variable = decl.VariableName });

                    ++i;

                    continue;
                }


                // <var-name> = <expression>
                if (t.tokenType == Models.TokenType.Identifier && arg[i + 1].IsOperator("="))
                {
                    ++i;
                    startIndex = ++i;

                    // No support for multiple assignments in decls
                    // The expression is in tokens startIndex to (i - 1), after forthcoming while exits
                    while (!arg[++i].IsSemicolon()) ;

                    // Parse the expression
                    var expr = ParseExpression(arg.Slice(startIndex, i - startIndex));

                    block.AddChild(new ArgonASTAssignment() { value = expr, variable = t.tokenValue });

                    ++i;

                    continue;
                }

                // <if> (<expression>) { ... }
                // <if> (<expression>) { ... } else { ... }
                if (arg[i].IsKeyword("if"))
                {
                    if (!arg[++i].IsOperator("("))
                        throw new InvalidProgramException($"Expected a '(' after 'if'. Found ${arg[i].tokenValue}.");

                    startIndex = ++i;

                    var ifBlock = new ArgonASTIf();

                    int depth = 1, endIndex = startIndex;
                    while (depth > 0)
                    {
                        if (arg[i].IsOperator("(")) depth++;
                        if (arg[i].IsOperator(")")) depth--;

                        ++i;

                        endIndex++;
                    }

                    var expr = ParseExpression(arg.Slice(startIndex, endIndex - startIndex - 1));
                    ifBlock.condition = expr;

                    // Goto next symbol after condition ')'
                    // ++i;

                    if (!arg[i].IsOperator("{"))
                        throw new InvalidProgramException($"Expected a '{{' for enclosing 'if' block. Found {arg[i].tokenValue}.");

                    startIndex = ++i;
                    depth = 1;
                    endIndex = startIndex;

                    while (depth > 0)
                    {
                        if (arg[i].IsOperator("{")) depth++;
                        if (arg[i].IsOperator("}")) depth--;

                        ++i;

                        endIndex++;
                    }

                    ifBlock.trueBlock = GenerateAST(arg.Slice(startIndex, endIndex - startIndex - 1));

                    // Check if EOF before checking for else
                    //++i;
                    if (i >= arg.Length)
                    {
                        block.AddChild(ifBlock);
                        continue;
                    }

                    if (arg[i].IsKeyword("else"))
                    {
                        ++i;

                        if (!arg[i].IsOperator("{"))
                            throw new InvalidProgramException($"Expected a '{{' for enclosing 'else' block. Found {arg[i].tokenValue}.");

                        startIndex = ++i;
                        depth = 1;
                        endIndex = startIndex;

                        while (depth > 0)
                        {
                            if (arg[i].IsOperator("{")) depth++;
                            if (arg[i].IsOperator("}")) depth--;

                            ++i;

                            endIndex++;
                        }

                        ifBlock.falseBlock = GenerateAST(arg.Slice(startIndex, endIndex - startIndex - 1));
                    }


                    block.AddChild(ifBlock);

                    continue;
                }

                // <while> (<expr>) { ... }
                if (arg[i].IsKeyword("while"))
                {
                    if (!arg[++i].IsOperator("("))
                        throw new InvalidProgramException($"Expected a '(' after 'while'. Found ${arg[i].tokenValue}.");

                    startIndex = ++i;

                    int depth = 1, endIndex = startIndex;
                    while (depth > 0)
                    {
                        if (arg[i].IsOperator("(")) depth++;
                        if (arg[i].IsOperator(")")) depth--;

                        ++i;

                        endIndex++;
                    }

                    var expr = ParseExpression(arg.Slice(startIndex, endIndex - startIndex - 1));
                    var whileBlock = new ArgonASTWhile(expr);

                    // Goto next symbol after condition ')'
                    // ++i;

                    if (!arg[i].IsOperator("{"))
                        throw new InvalidProgramException($"Expected a '{{' for enclosing 'if' block. Found {arg[i].tokenValue}.");

                    startIndex = ++i;
                    depth = 1;
                    endIndex = startIndex;

                    while (depth > 0)
                    {
                        if (arg[i].IsOperator("{")) depth++;
                        if (arg[i].IsOperator("}")) depth--;

                        ++i;

                        endIndex++;
                    }

                    whileBlock.loopBlock = GenerateAST(arg.Slice(startIndex, endIndex - startIndex - 1));

                    block.AddChild(whileBlock);
                }

                // <print> <expression>
                if (arg[i].IsKeyword("print"))
                {
                    startIndex = ++i;

                    // No support for multiple assignments in decls
                    // The expression is in tokens startIndex to (i - 1), after forthcoming while exits
                    while (!arg[++i].IsSemicolon()) ;

                    // Parse the expression
                    var expr = ParseExpression(arg.Slice(startIndex, i - startIndex));

                    block.AddChild(new ArgonASTPrint() { expression = expr });

                    ++i;

                    continue;
                }

                // <return> <expression>
                if (arg[i].IsKeyword("return"))
                {
                    startIndex = ++i;

                    // No support for multiple assignments in decls
                    // The expression is in tokens startIndex to (i - 1), after forthcoming while exits
                    while (!arg[++i].IsSemicolon()) ;

                    // Parse the expression
                    var expr = ParseExpression(arg.Slice(startIndex, i - startIndex));

                    block.AddChild(new ArgonASTReturn(expr));

                    ++i;

                    continue;
                }

                // <expression>
                startIndex = i;

                while (!arg[++i].IsSemicolon()) ;

                // Parse the expression
                var exp = ParseExpression(arg.Slice(startIndex, i - startIndex));

                block.AddChild(exp);

                ++i;

                continue;
            }

            return block;
        }


        // See https://en.wikipedia.org/wiki/Shunting-yard_algorithm
        // Basically, convert from infix to postfix, while creating the parse tree for the expression
        public static ValueContainer ParseExpression(Span<Models.Token> arg)
        {
            Stack<Models.Token> op_stack = new Stack<Models.Token>();
            Stack<ValueContainer> terminal_stack = new Stack<ValueContainer>();

            // Initialize with '(' so that no special cases exist
            op_stack.Push(new Models.Token() { tokenValue = "(", tokenType = Models.TokenType.Operator });

            for (int i = 0; i < arg.Length; i++)
            {
                switch (arg[i].tokenType)
                {
                    case Models.TokenType.Identifier:
                        terminal_stack.Push(new ArgonASTIdentifier(arg[i].tokenValue));
                        continue;
                    case Models.TokenType.StringLiteral:
                        terminal_stack.Push(new ArgonASTStringLiteral(arg[i].tokenValue));
                        continue;
                    case Models.TokenType.NumberLiteral:
                        terminal_stack.Push(new ArgonASTIntegerLiteral(int.Parse(arg[i].tokenValue)));
                        continue;
                }

                if (arg[i].IsOperator("("))
                {
                    // Check if previous token was an identifier, signalling a function call
                    // <identifier>() -> func call
                    if (arg[i - 1].tokenType == Models.TokenType.Identifier)
                    {
                        // Clear identifier from terminal stack
                        var functionName = ((ArgonASTIdentifier)terminal_stack.Pop()).VariableName;

                        var fcall = new ArgonASTFunctionCall(functionName);

                        // Parse parameters
                        int startIndex = ++i, depth = 1, length = 0;

                        while (depth > 0)
                        {
                            if (arg[i].IsOperator("(")) depth++;
                            if (arg[i].IsOperator(")")) depth--;

                            // Parameter delimiter
                            if ((arg[i].IsOperator(",") && depth == 1) || depth == 0)
                            {
                                if (length > 0)
                                    fcall.parameters.Add(ParseExpression(arg.Slice(startIndex, length)));

                                startIndex = i + 1;
                                length = -1;
                            }

                            ++i;
                            ++length;
                        }

                        terminal_stack.Push(fcall);

                        --i;
                    }
                    else
                        op_stack.Push(arg[i]);
                    continue;
                }

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

        private static ArgonASTBinaryOperator GetASTOperator(string op, ValueContainer x, ValueContainer y)
        {
            switch (op)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "==":
                case "<=":
                case ">=":
                case "!=":
                case "<":
                case ">":
                    // Switch the positions of left and right. 
                    // If not done, x - y shows up at y - x
                    return new ArgonASTBinaryOperator(op, y, x);
            }

            throw new NotImplementedException($"Operator {op} is not yet supported.");
        }
    }
}
