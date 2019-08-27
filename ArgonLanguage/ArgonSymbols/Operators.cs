using System.Collections.Generic;

namespace ArgonSymbols
{
    public static class Operators
    {
        private static List<string> list = new List<string>()
            { "+", "-" , "*", "/", "(", ")", ";", "+=", "-", "*=", "/=", "=", "{", "}", ","};

        public static bool IsOperator(string arg) => list.Contains(arg);
    }
}