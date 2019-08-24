using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonSymbols
{
    public static class Types
    {
        private static List<string> list = new List<string>()
            { "int", "string" };

        public static bool IsType(string arg) => list.Contains(arg);
    }
}
