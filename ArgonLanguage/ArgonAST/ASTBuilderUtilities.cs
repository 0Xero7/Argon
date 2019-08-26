using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonAST
{
    public static class ASTBuilderUtilities
    {
        public static bool IsSemicolon(this Models.Token token) => (token.tokenType == Models.TokenType.Operator && token.tokenValue == ";");
        public static bool IsOperator(this Models.Token token, string symbol) => (token.tokenType == Models.TokenType.Operator && token.tokenValue == symbol);
        public static bool IsKeyword(this Models.Token token, string keyword) => (token.tokenType == Models.TokenType.Keyword && token.tokenValue == keyword);
    }
}
