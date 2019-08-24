using System;
using System.ComponentModel;

namespace ArgonSymbols
{
    public static class Symbols
    {
        public static bool IsSymbol(string arg)
        {
            return Keywords.IsKeyword(arg) || Operators.IsOperator(arg) || Types.IsType(arg);
        }
        
        public static bool IsOperator(string arg)
        {
            return Operators.IsOperator(arg);
        }
        
        public static bool IsKeyword(string arg)
        {
            return Keywords.IsKeyword(arg);
        }

        public static bool IsType(string arg)
        {
            return Types.IsType(arg);
        }

        /// <summary>
        /// Returns the type of symbol, assuming parameter is a symbol.
        /// </summary>
        public static SymbolType GetSymbolType(string arg)
        {
            if (Keywords.IsKeyword(arg))
                return SymbolType.Keyword;
            if (Operators.IsOperator(arg))
                return SymbolType.Operator;
            if (Types.IsType(arg))
                return SymbolType.Type;

            throw new InvalidEnumArgumentException("Argument is not a valid symbol");
        }
    }
}