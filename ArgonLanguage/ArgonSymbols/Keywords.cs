using System.Collections.Generic;

namespace ArgonSymbols
{
    public static class Keywords
    {
        private static List<string> list = new List<string>()
            { "print" , "return", "if", "else", "for", "while" };
        
        public static bool IsKeyword(string arg) => list.Contains(arg);
    }
}