using System.Collections.Generic;
using System.Linq;

namespace ArgonSymbols
{
    public static class Operators
    {
        struct Operator
        {
            public string symbol;
            // 1 for Unary, 2 for Binary, 3 for Ternary
            public int type;
        }

        private static Dictionary<string, int> dict = new Dictionary<string, int>()
        {
            { "[", 0 },
            { "]", 0 },

            { "unary -", 1 },
            { "unary &", 1 },
            { "unary *", 1 },
            { "&", 1 },

            { "+", 2 },
            { "-", 2 },
            { "*", 2 },
            { "/", 2 },
            { "%", 2 },
            { "(", 2 },
            { ")", 2 },
            { ";", 2 },
            { "+=", 2 },
            { "-=", 2 },
            { "*=", 2 },
            { "/=", 2 },
            { "%=", 2 },
            { "=", 2 },
            { "==", 2 },
            { "<", 2 },
            { ">", 2 },
            { "<=", 2 },
            { ">=", 2 },
            { "!=", 2 },
            { "{", 2 },
            { "}", 2 },
            { ",", 2 }
        };


        public static bool IsOperator(string arg) => dict.ContainsKey(arg);

        public static int GetOperatorType(string arg) => dict[arg];
    }
}