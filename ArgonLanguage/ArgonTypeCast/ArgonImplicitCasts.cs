using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LLVMSharp;

namespace ArgonTypeCast
{
    public static class ArgonImplicitCasts
    {
        struct ImplicitCastDetails
        {
            public ImplicitCastDetails(params string[] args)
            {
                CanCastTo = args;
            }

            public string[] CanCastTo;
        };


        private static Dictionary<string, ImplicitCastDetails> ImplicitCasts = new System.Collections.Generic.Dictionary<string, ImplicitCastDetails>()
        {
            { "int",        new ImplicitCastDetails("float", "double") },
            { "float",      new ImplicitCastDetails("double") }
        };

        public static bool CanTypeBeImplicitlyCastTo(string from, string to)
        {
            if (!ImplicitCasts.ContainsKey(from))
                return false;

            return ImplicitCasts[from].CanCastTo.Contains(to);
        }

        public static LLVMValueRef ImplicitCastToFloat(LLVMValueRef arg, string type, LLVMBuilderRef b)
        {
            switch (type)
            {
                case "int":
                    return LLVM.BuildCast(b, LLVMOpcode.LLVMSIToFP, arg, LLVM.FloatType(), "");
                case "float":
                    return arg;
            }

            throw new NotSupportedException($"Cannot implicitly cast {type} to float.");
        }
    }
}
