using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonSymbols
{
    public static class Types
    {
        private static List<string> list = new List<string>()
            { "int", "float", "string", "bool", "void" };

        public static bool IsType(string arg) => list.Contains(arg);
    }
}
