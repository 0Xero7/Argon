using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonSymbolTable
{
    public static class SymbolsTable
    {
        struct FunctionDetails
        {
            public FunctionDetails(string returnType, bool IsVarArg, LLVMSharp.LLVMValueRef functionReference)
            {
                this.returnType = returnType;
                this.IsVarArg = IsVarArg;
                this.functionReference = functionReference;
            }

            public string returnType;
            public bool IsVarArg;
            public LLVMSharp.LLVMValueRef functionReference;
        };

        private static Dictionary<string, FunctionDetails> Functions
            = new Dictionary<string, FunctionDetails>();

        public static void AddFunctionToScope(string name, string returnType, LLVMSharp.LLVMValueRef functionReference, bool IsVarArgs)
        {
            Functions.Add(name, new FunctionDetails(returnType, IsVarArgs, functionReference));
        }

        public static string GetFunctionReturnType(string name)
        {
            return Functions[name].returnType;
        }

        public static LLVMSharp.LLVMValueRef GetFunctionReference(string name)
        {
            return Functions[name].functionReference;
        }

        public static bool GetFunctionIsVarArgs(string name)
        {
            return Functions[name].IsVarArg;
        }








        //    V A R I A B L E S      T A B L E

        public struct VariableDetails
        {
            public VariableDetails(string type, bool IsPtr, int valuePtrDepth, LLVMSharp.LLVMValueRef reference)
            {
                this.type = type;
                this.IsPtr = IsPtr;
                this.valuePtrDepth = valuePtrDepth;
                this.reference = reference;
            }

            public string type;
            public bool IsPtr;
            public int valuePtrDepth;
            public LLVMSharp.LLVMValueRef reference;
        };

        private static Dictionary<string, VariableDetails> Variables =
            new Dictionary<string, VariableDetails>();

        public static void AddVariableToScope(string name, string type, LLVMSharp.LLVMValueRef reference, int valuePtrDepth, bool IsPtr)
        {
            Variables.Add(name, new VariableDetails(type, IsPtr, valuePtrDepth, reference));
        }

        public static VariableDetails TestReturn(string arg)
        {
            return Variables[arg];
        }

        public static string GetVariableType(string name)
        {
            return Variables[name].type;
        }

        public static LLVMSharp.LLVMValueRef GetVariableReference(string name)
        {
            return Variables[name].reference;
        }

        public static bool IsVariablePtr(string name)
        {
            return Variables[name].IsPtr;
        }

    }
}
