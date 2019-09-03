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
            { "void", VoidType() },
            { "string", PointerType(Int8Type(), 0) }
        };

        public static Dictionary<string, (string type, LLVMSharp.LLVMValueRef vref, bool IsPtr)> Variables = 
            new Dictionary<string, (string type, LLVMSharp.LLVMValueRef vref, bool IsPtr)>();

        public static Dictionary<string, (string type, LLVMSharp.LLVMValueRef vref)> Functions 
            = new Dictionary<string, (string type, LLVMSharp.LLVMValueRef vref)>();
    }
}
