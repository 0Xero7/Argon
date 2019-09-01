using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonSymbols
{
    public static class Precedences
    {
        public static int GetPrecedence(string op)
        {
            try
            {
                return precedenceDict[op];
            }
            catch
            {
                throw new NotImplementedException($"Operator {op} not yet supported.");
            }
        }

        private static Dictionary<string, int> precedenceDict = new Dictionary<string, int>()
        {
            { "(", -9999 },
            { "=", -9998 },
            { "==", 0 },
            { "<=", 0 },
            { ">=", 0 },
            { "!=", 0 },
            { "<", 0 },
            { ">", 0 },
            { "+" , 1 },
            { "-" , 1 },
            { "*" , 2 },
            { "/" , 2 }
        };
    }
}
