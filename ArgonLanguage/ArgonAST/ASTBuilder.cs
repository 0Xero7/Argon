using System;
using System.Collections.Generic;
using System.Text;

using ArgonASTModels;
using ArgonASTModels.ValueTypes;
using static ArgonAST.ASTBuilderUtilities;
using TokenStream;
using Models;

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

        public static ArgonASTBlock GenerateAST(ArgonTokenStream arg)
        {
            int i = 0;
            int startIndex = 0;

            var block = new ArgonASTBlock();

            while (!arg.IsAtEnd())
            {
                var t = arg.CurrentToken;

                // Function Decl 
                // <ret-type> <func-name>(<type1> <var-name1>, <type2> <var-name2>,...) { ... }
                if (t.tokenType == Models.TokenType.Type && arg.NextToken.tokenType == Models.TokenType.Identifier && arg.PeekAhead(2).IsOperator("("))
                {
                    arg.ConsumeToken();
                    var decl = new ArgonASTFunctionDeclaration(t.tokenValue, arg.ConsumeToken().tokenValue);

                    // Read the formal parameters
                    while (!arg.ConsumeToNext().IsOperator(")"))
                    {
                        var type = arg.CurrentToken.tokenValue;
                        var name = arg.ConsumeToNext().tokenValue;

                        if (arg.NextToken.IsOperator(","))
                            arg.ConsumeToken();

                        var param = new ArgonASTDeclaration(type, name);

                        decl.FormalParamaters.Add(param);
                    }

                    // Capture and recursively parse the body of the function

                    // No function body, throw exception
                    if (!arg.ConsumeToNext().IsOperator("{"))
                        throw new InvalidProgramException($"Function body doesn't have a '{{'. Found {arg.CurrentToken.tokenValue}.");

                    decl.FunctionBody = ParseBlock(arg);

                    block.AddChild(decl);
                    continue;
                }


                // <continue>
                if (arg.CurrentToken.IsKeyword("continue") && arg.NextToken.IsOperator(";"))
                {
                    arg.ConsumeToken();
                    arg.ConsumeToken();

                    block.AddChild(new ArgonASTContinue());
                    continue;
                }
            }

            return block;
        }

        public static ArgonASTBlock ParseBlock(ArgonTokenStream arg)
        {
            ArgonASTBlock block = new ArgonASTBlock();

            // A multiline block
            if (arg.CurrentToken.IsOperator("{"))
            {
                arg.ConsumeToken();

                int depth = 1;
                arg.StartSlice();

                while (depth > 0)
                {
                    if (arg.CurrentToken.IsOperator("{")) ++depth;
                    if (arg.CurrentToken.IsOperator("}")) --depth;

                    arg.ConsumeToken();
                }

                var body = arg.FinishSlice(1);

                while (!body.IsAtEnd())
                    ParseUnit(block, body);
            }
            else  // Single line block
            {
                ParseUnit(block, arg);
            }

            return block;
        }

        // <type> <var-name>
        // <type> <var-name> = <expression>
        public static void ParseVariableDecl(ArgonASTBlock block, ArgonTokenStream arg)
        {
            var type = arg.CurrentToken.tokenValue;
            int ptrDepth = 0;

            // There might be other symbols between the typename and the variable name, such as '*' in 'int *x'
            while (arg.ConsumeToNext().IsOperator("*"))
            {
                ++ptrDepth;
                type += "*";
            }

            var decl = new ArgonASTDeclaration(type, arg.CurrentToken.tokenValue, ptrDepth);
            block.AddChild(decl);

            // No initialization
            if (arg.ConsumeToNext().IsSemicolon())
            { arg.ConsumeToken(); return; }

            // It is an initialization
            // Break it down into TWO entries, a decl (which has been created) and the assignment

            // Expecting a '=', throw a helpful exception if something else is encountered
            if (!arg.CurrentToken.IsOperator("="))
                throw new InvalidOperationException($"Unexpected symbol '{arg.CurrentToken.tokenValue}' at line {arg.CurrentToken.lineNumber}. Expecting '='.");

            //startIndex = ++i;
            arg.ConsumeToken();
            arg.StartSlice();

            // No support for multiple assignments in decls
            // The expression is in tokens startIndex to (i - 1), after forthcoming while exits
            while (!arg.ConsumeToNext().IsSemicolon()) ;

            // Parse the expression
            var expr = ParseExpression(arg.FinishSlice());//.Slice(startIndex, i - startIndex));

            block.AddChild(new ArgonASTAssignment() { value = expr, variable = decl.VariableName });

            arg.ConsumeToken();
        }

        // <if> (<expression>) { ... }
        // <if> (<expression>) { ... } else { ... }
        public static void ParseIf(ArgonASTBlock block, ArgonTokenStream arg)
        {
            if (!arg.ConsumeToNext().IsOperator("("))
                throw new InvalidProgramException($"Expected a '(' after 'if'. Found ${arg.CurrentToken.tokenValue}.");

            //startIndex = ++i;
            arg.ConsumeToken();
            arg.StartSlice();

            var ifBlock = new ArgonASTIf();

            int depth = 1;
            while (depth > 0)
            {
                if (arg.CurrentToken.IsOperator("(")) depth++;
                if (arg.CurrentToken.IsOperator(")")) depth--;

                arg.ConsumeToken();
            }

            var expr = ParseExpression(arg.FinishSlice());// arg.Slice(startIndex, endIndex - startIndex - 1));
            ifBlock.condition = expr;

            // Goto next symbol after condition ')'
            // ++i;

            if (!arg.CurrentToken.IsOperator("{"))
            {
                ifBlock.trueBlock = new ArgonASTBlock();
                ParseUnit(ifBlock.trueBlock, arg);
            }
            else
                ifBlock.trueBlock = ParseBlock(arg);

            // Check if EOF before checking for else
            if (arg.IsAtEnd())
            {
                block.AddChild(ifBlock);
                return;
            }

            if (arg.CurrentToken.IsKeyword("else"))
            {
                arg.ConsumeToken();

                if (!arg.CurrentToken.IsOperator("{"))
                {
                    ifBlock.falseBlock = new ArgonASTBlock();
                    ParseUnit(ifBlock.falseBlock, arg);
                }
                else
                    ifBlock.falseBlock = ParseBlock(arg);
            }

            block.AddChild(ifBlock);
        }

        public static void ParseWhile(ArgonASTBlock block, ArgonTokenStream arg)
        {
            if (!arg.ConsumeToNext().IsOperator("("))
                throw new InvalidProgramException($"Expected a '(' after 'while'. Found ${arg.CurrentToken.tokenValue}.");

            //startIndex = ++i;
            arg.ConsumeToken();
            arg.StartSlice();

            int depth = 1;
            while (depth > 0)
            {
                if (arg.CurrentToken.IsOperator("(")) depth++;
                if (arg.CurrentToken.IsOperator(")")) depth--;

                arg.ConsumeToken();
            }

            var expr = ParseExpression(arg.FinishSlice(1));
            var whileBlock = new ArgonASTWhile(expr);

            // Goto next symbol after condition ')'
            // ++i;

            if (!arg.CurrentToken.IsOperator("{"))
            {
                whileBlock.loopBlock = new ArgonASTBlock();
                ParseUnit(whileBlock.loopBlock, arg);
            }
            else
                whileBlock.loopBlock = ParseBlock(arg);

            block.AddChild(whileBlock);
        }

        public static void ParseFor(ArgonASTBlock block, ArgonTokenStream arg)
        {
            var forBlock = new ArgonASTFor();

            if (!arg.ConsumeToNext().IsOperator("("))
                throw new InvalidProgramException($"Expected a '(' after 'for'. Found ${arg.CurrentToken.tokenValue} at line ${arg.CurrentToken.lineNumber}.");

            arg.ConsumeToken();

            // Get expr1
            ParseUnit(forBlock.init, arg);

            // Get boolean
            arg.StartSlice();

            while (!arg.ConsumeToken().IsSemicolon()) ;
            var boolean = arg.FinishSlice(1);

            forBlock.conditional = ParseExpression(boolean);

            // Get expr2
            arg.StartSlice();

            int depth = 1;

            while (depth > 0)
            {
                if (arg.CurrentToken.IsOperator("(")) depth++;
                if (arg.CurrentToken.IsOperator(")")) depth--;

                arg.ConsumeToken();
            }

            var expr2 = arg.FinishSlice(1);
            expr2.AddToken(new Token() { lineNumber = 0, tokenType = TokenType.Operator, tokenValue = ";" });

            ParseUnit(forBlock.increment, expr2);

            forBlock.body = ParseBlock(arg);

            block.AddChild(forBlock);

            return;
        }

        public static void ParseReturn(ArgonASTBlock block, ArgonTokenStream arg)
        {
            //startIndex = ++i;
            arg.ConsumeToken();
            arg.StartSlice();

            // No support for multiple assignments in decls
            // The expression is in tokens startIndex to (i - 1), after forthcoming while exits
            while (!arg.ConsumeToNext().IsSemicolon()) ;

            // Parse the expression
            var expr = ParseExpression(arg.FinishSlice());// Slice(startIndex, i - startIndex));

            block.AddChild(new ArgonASTReturn(expr));

            arg.ConsumeToken();

            return;
        }

        public static void ParseExpression(ArgonASTBlock block, ArgonTokenStream arg)
        {
            arg.StartSlice();

            while (!arg.ConsumeToNext().IsSemicolon()) ;

            // Parse the expression
            var exp = ParseExpression(arg.FinishSlice());// Slice(startIndex, i - startIndex));

            block.AddChild(exp);

            arg.ConsumeToken();
        }

        public static void ParseUnit(ArgonASTBlock block, ArgonTokenStream arg)
        {
            var t = arg.CurrentToken;

            // Variable Decl 
            // <type> <var-name>
            // <type> <var-name> = <expression>
            if (t.tokenType == Models.TokenType.Type)
            {
                ParseVariableDecl(block, arg);
                return;
            }

            // <if> (<expression>) { ... }
            // <if> (<expression>) { ... } else { ... }
            if (arg.CurrentToken.IsKeyword("if"))
            {
                ParseIf(block, arg);
                return;
            }

            // <for> (<expr1> ; <boolean> ; <expr2>) { ... }
            if (arg.CurrentToken.IsKeyword("for"))
            {
                ParseFor(block, arg);
                return;
            }

            // <while> (<expr>) { ... }
            if (arg.CurrentToken.IsKeyword("while"))
            {
                ParseWhile(block, arg);
                return;
            }

            // <continue>
            if (arg.CurrentToken.IsKeyword("continue") && arg.NextToken.IsOperator(";"))
            {
                arg.ConsumeToken();
                arg.ConsumeToken();

                block.AddChild(new ArgonASTContinue());
                return;
            }

            // <return> <expression>
            if (arg.CurrentToken.IsKeyword("return"))
            {
                ParseReturn(block, arg);
                return;
            }

            // <expression>
            ParseExpression(block, arg);
        }


        // See https://en.wikipedia.org/wiki/Shunting-yard_algorithm
        // Basically, convert from infix to postfix, while creating the parse tree for the expression
        public static ValueContainer ParseExpression(ArgonTokenStream arg)
        {
            Stack<Token> opStack = new Stack<Token>();
            Stack<ValueContainer> termStack = new Stack<ValueContainer>();

            // Initialize with '(' so that no special cases exist
            opStack.Push(new Models.Token() { tokenValue = "(", tokenType = Models.TokenType.Operator });

            while (!arg.IsAtEnd())
            {
                switch (arg.CurrentToken.tokenType)
                {
                    case Models.TokenType.Identifier:
                        termStack.Push(new ArgonASTIdentifier(arg.ConsumeToken().tokenValue));
                        continue;
                    case Models.TokenType.StringLiteral:
                        termStack.Push(new ArgonASTStringLiteral(arg.ConsumeToken().tokenValue));
                        continue;
                    case Models.TokenType.NumberLiteral:
                        termStack.Push(ResolveNumberType(arg.ConsumeToken().tokenValue));
                        continue;
                }

                // It's a function call
                // <identifier> ()
                if (arg.CurrentToken.IsOperator("(") && arg.PreviousToken.IsIdentifier())
                {
                    var functionName = (termStack.Pop() as ArgonASTIdentifier).VariableName;
                    var fcall = new ArgonASTFunctionCall(functionName);

                    // Consume the '(' and point to the next token
                    int depth = 1, paramLength = 0;
                    arg.ConsumeToken();

                    arg.StartSlice();

                    // Slice the parameters out together, when this exits the 
                    // stream points the token right after the closing '('
                    while (depth > 0)
                    {
                        if (arg.CurrentToken.IsOperator("(")) depth++;
                        if (arg.CurrentToken.IsOperator(")")) depth--;

                        arg.ConsumeToken();
                    }

                    // We have sliced out all the parameters
                    var parameters = arg.FinishSlice(1);

                    depth = 0;
                    parameters.StartSlice();
                    // Fish out the individual parameters
                    while (!parameters.IsAtEnd())
                    {
                        if (parameters.CurrentToken.IsOperator("(")) depth++;
                        if (parameters.CurrentToken.IsOperator(")")) depth--;

                        if (parameters.CurrentToken.IsOperator(",") && depth == 0)
                        {
                            fcall.parameters.Add(ParseExpression(parameters.FinishSlice()));
                            parameters.ConsumeToken();
                            parameters.StartSlice();
                        }
                        else
                            parameters.ConsumeToken();
                    }

                    fcall.parameters.Add(ParseExpression(parameters.FinishSlice()));

                    termStack.Push(fcall);

                    arg.ConsumeToken();

                    continue;
                }

                // Symbol is ** Operator **

                if (arg.CurrentToken.IsOperator("("))
                {
                    opStack.Push(arg.ConsumeToken());
                    continue;
                }

                if (arg.CurrentToken.IsOperator(")"))
                {
                    while (!opStack.Peek().IsOperator("("))
                    {
                        switch (ArgonSymbols.Operators.GetOperatorType(opStack.Peek().tokenValue))
                        {
                            case 1:
                                var a = termStack.Pop();

                                termStack.Push(GetASTUnaryOperator(opStack.Pop().tokenValue, a));
                                break;

                            case 2:
                                var x = termStack.Pop();
                                var y = termStack.Pop();

                                termStack.Push(GetASTBinaryOperator(opStack.Pop().tokenValue, x, y));
                                break;
                        }
                    }

                    // Remove the '(' from the stack
                    opStack.Pop();
                    arg.ConsumeToken();

                    continue;
                }

                while (ArgonSymbols.Precedences.GetPrecedence(opStack.Peek().tokenValue)
                    >= ArgonSymbols.Precedences.GetPrecedence((arg.PreviousToken.IsOperator() || arg.IsAtStart()) ?
                    "unary " + arg.CurrentToken.tokenValue :
                    arg.CurrentToken.tokenValue))
                {
                    var ops = opStack.Pop();

                    switch (ArgonSymbols.Operators.GetOperatorType(ops.tokenValue))
                    {
                        case 1:
                            var x1 = termStack.Pop();

                            termStack.Push(GetASTUnaryOperator(ops.tokenValue, x1));
                            break;

                        case 2:
                            var x = termStack.Pop();
                            var y = termStack.Pop();

                            termStack.Push(GetASTBinaryOperator(ops.tokenValue, x, y));
                            break;
                    }
                }

                if (arg.PreviousToken.IsOperator() || arg.IsAtStart())
                    opStack.Push(new Models.Token() { lineNumber = arg.CurrentToken.lineNumber, tokenType = Models.TokenType.Operator, tokenValue = $"unary {arg.CurrentToken.tokenValue}" });
                else
                    opStack.Push(arg.CurrentToken);

                arg.ConsumeToken();
            }

            while (opStack.Count > 0 && !opStack.Peek().IsOperator("("))
            {
                switch (ArgonSymbols.Operators.GetOperatorType(opStack.Peek().tokenValue))
                {
                    case 1:
                        var a = termStack.Pop();

                        termStack.Push(GetASTUnaryOperator(opStack.Pop().tokenValue, a));
                        break;

                    case 2:
                        var x = termStack.Pop();
                        var y = termStack.Pop();

                        termStack.Push(GetASTBinaryOperator(opStack.Pop().tokenValue, x, y));
                        break;
                }
            }

            return termStack.Pop();
        }

        private static ArgonASTUnaryOperator GetASTUnaryOperator(string op, ValueContainer x)
        {
            switch (op)
            {
                case "unary -":
                    return new ArgonASTUnaryOperator("-", x);

                case "unary &":
                    return new ArgonASTUnaryOperator("&", x);

                case "unary *":
                    return new ArgonASTUnaryOperator("*", x);
            }

            throw new NotImplementedException($"Unary operator {op} is not yet supported.");
        }

        private static ArgonASTBinaryOperator GetASTBinaryOperator(string op, ValueContainer x, ValueContainer y)
        {
            switch (op)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "%":
                case "==":
                case "<=":
                case ">=":
                case "!=":
                case "<":
                case ">":
                case "=":
                    // Switch the positions of left and right. 
                    // If not done, x - y shows up at y - x
                    return new ArgonASTBinaryOperator(op, y, x);
            }

            throw new NotImplementedException($"Binary operator {op} is not yet supported.");
        }

        private static ValueContainer ResolveNumberType(string arg)
        {
            // Is a float or double
            if (arg.Contains('.'))
            {
                if (float.TryParse(arg, out float f))
                    return new ArgonASTFloatLiteral(f);

                throw new NotImplementedException($"Cannot parse {arg} as a number.");
            }
            else  // Is a integer variation
            {
                if (int.TryParse(arg, out int i))
                    return new ArgonASTIntegerLiteral(i);

                throw new NotImplementedException($"Cannot parse {arg} as a number.");
            }
        }
    }
}
