using System;
using System.Collections.Generic;
using System.Text;
using ArgonASTModels;

using static LLVMSharp.LLVM;

namespace ArgonCodeGen
{
    public static class SymbolsTable
    {
        public static Dictionary<string, LLVMSharp.LLVMTypeRef> AST2LLVMTypes = new Dictionary<string, LLVMSharp.LLVMTypeRef>()
        {
            { "int", Int32Type() },
            { "float", FloatType() },
            { "void", VoidType() },
            { "string", PointerType(Int8Type(), 0) }
        };

        public static LLVMSharp.LLVMTypeRef GetLLVMType(string arg)
        {
            if (!arg.EndsWith('*'))
                return AST2LLVMTypes[arg];

            var startIndex = arg.IndexOf('*');
            var type = arg.Substring(0, startIndex);
            var llvmtype = AST2LLVMTypes[type];

            for (int i = 0; i < arg.Length - startIndex; i++)
                llvmtype = PointerType(llvmtype, 0);

            return llvmtype;
        }
    }
}
