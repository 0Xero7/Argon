using ArgonASTModels;
using ArgonASTModels.ValueTypes;

using LLVMSharp;

using System;
using System.Collections.Generic;

using static ArgonCodeGen.SymbolsTable;
using static ArgonSymbolTable.SymbolsTable;
using static LLVMSharp.LLVM;

namespace ArgonCodeGen
{
    public static class ArgonCodeGen
    {
        private static LLVMValueRef ConstIntZero = LLVM.ConstInt(Int32Type(), 0, true);

        private static LLVMValueRef thisFuncRetVal = ConstIntZero;
        private static LLVMBasicBlockRef thisFuncRetBlock;

        private static Stack<LLVMBasicBlockRef> loopContinueStack;
        private static Stack<LLVMBasicBlockRef> loopBreakStack;

        public static void GetGeneratedCode(ArgonASTBase arg)
        {
            var context = ContextCreate();
            var module = LLVM.ModuleCreateWithNameInContext("top", context);
            var builder = LLVM.CreateBuilderInContext(context);

            loopContinueStack = new Stack<LLVMBasicBlockRef>();
            loopBreakStack = new Stack<LLVMBasicBlockRef>();

            var block = arg as ArgonASTBlock;

            // Printf
            var printfPType = new LLVMTypeRef[1] { PointerType(Int8Type(), 0) };
            var printfFType = FunctionType(Int32Type(), printfPType, true);
            var printfFunction = LLVM.AddFunction(module, "printf", printfFType);

            AddFunctionToScope("printf", "int", printfFunction, true);

            // Input Integer
            //var scanfPType = new LLVMTypeRef[1] { PointerType(Int8Type(), 0) };
            //var scanfFType = FunctionType(Int32Type(), scanfPType, true);
            //var scanfFunction = LLVM.AddFunction(module, "__isoc99_scanf", scanfFType);

            //AddFunctionToScope("__isoc99_scanf", "int", scanfFunction, true);



            //var readIntType = new LLVMTypeRef[0] { };
            //var readIntFType = FunctionType(Int32Type(), readIntType, false);
            //var readIntFunction = LLVM.AddFunction(module, "ReadInt", readIntFType);

            //AddFunctionToScope("ReadInt", "int", scanfFunction, false);

            //var rbb = AppendBasicBlock(readIntFunction, "");
            //PositionBuilderAtEnd(builder, rbb);
            //var store = BuildAlloca(builder, Int32Type(), "");
            //var str = BuildGlobalStringPtr(builder, "%d", "");
            //BuildCall(builder, scanfFunction, new LLVMValueRef[2] {  str, store }, "");
            //BuildRet(builder, BuildLoad(builder, store, ""));



            // Forward Declare Functions
            foreach (var v in block.Children)
            {
                switch (v)
                {
                    case ArgonASTFunctionDeclaration f:
                        var p = new LLVMTypeRef[f.FormalParamaters.Count];
                        int i = 0;
                        foreach (var vx in f.FormalParamaters)
                            p[i++] = AST2LLVMTypes[vx.Type];

                        var x = LLVM.FunctionType(AST2LLVMTypes[f.ReturnType], p, false);

                        var fn = LLVM.AddFunction(module, f.FunctionName, x);

                        AddFunctionToScope(f.FunctionName, f.ReturnType, fn, false);


                        //SymbolsTable.Functions.Add(f.FunctionName, (f.ReturnType, fn, false));
                        break;
                }
            }

            foreach (var v in block.Children)
            {
                switch (v)
                {
                    case ArgonASTFunctionDeclaration fdcl:
                        GIRFuncDecl(fdcl, module, builder);
                        break;
                }
            }

            LLVM.DumpModule(module);
            LLVM.PrintModuleToFile(module, "D:\\llvmtest\\main.ll", out string err);
        }

        private static void GIRBlock(ArgonASTBlock block, LLVMModuleRef m, LLVMBuilderRef b, LLVMValueRef fn, LLVMBasicBlockRef thisBlock)
        {
            if (block == null) return;
            foreach (var t in block.Children)
            {
                switch (t)
                {
                    case ArgonASTDeclaration decl:
                        var vb = BuildAlloca(b, GetLLVMType(decl.Type), "");
                        AddVariableToScope(decl.VariableName, decl.Type, vb, decl.ptrDepth, true);

                        var khs = TestReturn(decl.VariableName);


                        //Variables.Add(decl.VariableName, (decl.Type, vb, true));
                        break;

                    case ArgonASTAssignment ass:

                        Console.WriteLine(GetVariableType(ass.variable) + " - " + ArgonTypeResolver.ArgonTypeResolver.GetType(ass.value));

                        var h = GIRValueType(ass.value, m, b);

                        if (GetVariableType(ass.variable) != ArgonTypeResolver.ArgonTypeResolver.GetType(ass.value))
                        {
                            if (GetVariableType(ass.variable) == "float" && ArgonTypeResolver.ArgonTypeResolver.GetType(ass.value) == "int")
                                h = BuildCast(b, LLVMOpcode.LLVMSIToFP, h, FloatType(), "");
                        }

                        BuildStore(b, h, GetVariableReference(ass.variable));

                        break;

                    case ArgonASTFunctionCall call:
                        GIRFuncCall(call, m, b);
                        break;

                    case ArgonASTIf iff:
                        GIRIfElse(iff, m, b, fn, thisBlock);
                        break;

                    case ArgonASTWhile whle:
                        GIRWhile(whle, m, b, fn, thisBlock);
                        break;

                    case ArgonASTFor fr:
                        GIRFor(fr, m, b, fn, thisBlock);
                        break;

                    case ArgonASTContinue cnt:
                        GIRContinue(b);
                        break;

                    case ArgonASTReturn ret:
                        var rp = ret.expression;
                        //BuildRet(b, GIRValueType(rp, m, b));
                        BuildStore(b, GIRValueType(rp, m, b), thisFuncRetVal);
                        BuildBr(b, thisFuncRetBlock);
                        break;

                    case ArgonASTBinaryOperator bop:
                        GIRBinOp(bop, m, b);
                        break;
                }
            }
        }

        private static void GIRFuncDecl(ArgonASTFunctionDeclaration f, LLVMModuleRef m, LLVMBuilderRef b)
        {
            LLVMValueRef returnSt = ConstIntZero, retValue = ConstIntZero;

            var bb = AppendBasicBlock(GetFunctionReference(f.FunctionName), "");
            var endbb = AppendBasicBlock(GetFunctionReference(f.FunctionName), "");

            if (f.ReturnType != "void")
            {
                PositionBuilderAtEnd(b, bb);
                retValue = LLVM.BuildAlloca(b, SymbolsTable.AST2LLVMTypes[f.ReturnType], "");

                PositionBuilderAtEnd(b, endbb);
                returnSt = BuildRet(b, BuildLoad(b, retValue, ""));

                PositionBuilderAtEnd(b, bb);
            }
            else
            {
                PositionBuilderAtEnd(b, endbb);
                BuildRetVoid(b);

                PositionBuilderAtEnd(b, bb);
            }

            thisFuncRetBlock = endbb;
            thisFuncRetVal = retValue;

            var parameters = GetFunctionReference(f.FunctionName).GetParams();
            int ix = 0;
            foreach (var x in parameters)
                AddVariableToScope(f.FormalParamaters[ix].VariableName, f.FormalParamaters[ix++].Type, x, 0, false);

            // SymbolsTable.Variables.Add(f.FormalParamaters[ix].VariableName, (f.FormalParamaters[ix++].Type, x, false));


            GIRBlock(f.FunctionBody, m, b, GetFunctionReference(f.FunctionName), bb);

            var sxx = GetLastInstruction(GetLastBasicBlock(GetFunctionReference(f.FunctionName)).GetPreviousBasicBlock());
            if (!LLVM.IsABranchInst(sxx).ToString().Trim().StartsWith("br"))
            {
                PositionBuilderAtEnd(b, GetLastBasicBlock(GetFunctionReference(f.FunctionName)).GetPreviousBasicBlock());
                BuildBr(b, endbb);
            }

            ////var list = GetBasicBlocks(SymbolsTable.Functions[f.FunctionName].vref);
            //PositionBuilderAtEnd(b, GetLastBasicBlock(SymbolsTable.Functions[f.FunctionName].vref));
            //BuildBr(b, thisFuncRetBlock);
        }

        public static void GIRIfElse(ArgonASTIf iff, LLVMModuleRef m, LLVMBuilderRef b, LLVMValueRef fn,
            LLVMBasicBlockRef thisBlock)
        {
            var doesTrueReturn = ArgonBranchAnalyzer.ArgonBranchChangesControlFlow.DoesBranchChangeControlFlow(iff.trueBlock);
            var doesFalseReturn = ArgonBranchAnalyzer.ArgonBranchChangesControlFlow.DoesBranchChangeControlFlow(iff.falseBlock);

            // Only Then Block
            if (iff.falseBlock == null)
            {
                var cnd = GIRValueType(iff.condition, m, b);

                var thenBlock = thisBlock.InsertBasicBlock("then");
                var mergeBlock = thisBlock.InsertBasicBlock("merge");

                thenBlock.MoveBasicBlockAfter(thisBlock);
                mergeBlock.MoveBasicBlockAfter(thenBlock);

                BuildCondBr(b, cnd, thenBlock, mergeBlock);

                PositionBuilderAtEnd(b, thenBlock);
                GIRBlock(iff.trueBlock, m, b, fn, thenBlock);

                if (!doesTrueReturn)
                    BuildBr(b, mergeBlock);

                PositionBuilderAtEnd(b, mergeBlock);
            }
            else   // Both Then and Else Block
            {
                var cnd = GIRValueType(iff.condition, m, b);

                // One of the blocks does not return
                if (!(doesTrueReturn && doesFalseReturn))
                {
                    var thenBlock = thisBlock.InsertBasicBlock("then");
                    var elseBlock = thisBlock.InsertBasicBlock("else");
                    var mergeBlock = thisBlock.InsertBasicBlock("merge");

                    thenBlock.MoveBasicBlockAfter(thisBlock);
                    elseBlock.MoveBasicBlockAfter(thenBlock);
                    mergeBlock.MoveBasicBlockAfter(elseBlock);

                    BuildCondBr(b, cnd, thenBlock, elseBlock);

                    PositionBuilderAtEnd(b, thenBlock);
                    GIRBlock(iff.trueBlock, m, b, fn, thenBlock);
                    if (!doesTrueReturn)
                        BuildBr(b, mergeBlock);

                    PositionBuilderAtEnd(b, elseBlock);
                    GIRBlock(iff.falseBlock, m, b, fn, elseBlock);
                    if (!doesFalseReturn)
                        BuildBr(b, mergeBlock);

                    PositionBuilderAtEnd(b, mergeBlock);
                }
                else     // Both blocks return, no need for merge block
                {
                    var nextBlock = thisBlock.GetNextBasicBlock();

                    var thenBlock = thisBlock.InsertBasicBlock("then");
                    var elseBlock = thisBlock.InsertBasicBlock("else");

                    thenBlock.MoveBasicBlockAfter(thisBlock);
                    elseBlock.MoveBasicBlockAfter(thenBlock);

                    BuildCondBr(b, cnd, thenBlock, elseBlock);

                    PositionBuilderAtEnd(b, thenBlock);
                    GIRBlock(iff.trueBlock, m, b, fn, thenBlock);

                    PositionBuilderAtEnd(b, elseBlock);
                    GIRBlock(iff.falseBlock, m, b, fn, elseBlock);

                    PositionBuilderAtEnd(b, nextBlock);
                }
            }
        }

        public static void GIRContinue(LLVMBuilderRef b)
        {
            BuildBr(b, loopContinueStack.Peek());
        }

        public static LLVMValueRef GIRWhile(ArgonASTWhile whl, LLVMModuleRef m, LLVMBuilderRef b, LLVMValueRef fn,
            LLVMBasicBlockRef thisBlock)
        {
            var entryBlock = thisBlock.InsertBasicBlock("");
            var loopBlock = thisBlock.InsertBasicBlock("");
            var exitBlock = thisBlock.InsertBasicBlock("");

            loopContinueStack.Push(entryBlock);
            loopBreakStack.Push(exitBlock);

            entryBlock.MoveBasicBlockAfter(thisBlock);
            loopBlock.MoveBasicBlockAfter(entryBlock);
            exitBlock.MoveBasicBlockAfter(loopBlock);

            BuildBr(b, entryBlock);

            PositionBuilderAtEnd(b, entryBlock);
            var cnd = GIRValueType(whl.condition, m, b);
            BuildCondBr(b, cnd, loopBlock, exitBlock);

            PositionBuilderAtEnd(b, loopBlock);
            GIRBlock(whl.loopBlock, m, b, fn, loopBlock);
            BuildBr(b, entryBlock);

            PositionBuilderAtEnd(b, exitBlock);

            loopContinueStack.Pop();
            loopBreakStack.Pop();

            return cnd;
        }
        public static LLVMValueRef GIRFor(ArgonASTFor fr, LLVMModuleRef m, LLVMBuilderRef b, LLVMValueRef fn,
            LLVMBasicBlockRef thisBlock)
        {
            var entryBlock = thisBlock.InsertBasicBlock("");
            var loopBlock = thisBlock.InsertBasicBlock("");
            var exitBlock = thisBlock.InsertBasicBlock("");

            loopContinueStack.Push(entryBlock);
            loopBreakStack.Push(exitBlock);

            entryBlock.MoveBasicBlockAfter(thisBlock);
            loopBlock.MoveBasicBlockAfter(entryBlock);
            exitBlock.MoveBasicBlockAfter(loopBlock);

            GIRBlock(fr.init, m, b, fn, thisBlock);
            BuildBr(b, entryBlock);

            PositionBuilderAtEnd(b, entryBlock);
            var cnd = GIRValueType(fr.conditional, m, b);
            BuildCondBr(b, cnd, loopBlock, exitBlock);

            PositionBuilderAtEnd(b, loopBlock);
            GIRBlock(fr.body, m, b, fn, loopBlock);
            GIRBlock(fr.increment, m, b, fn, loopBlock);
            BuildBr(b, entryBlock);

            PositionBuilderAtEnd(b, exitBlock);

            loopContinueStack.Pop();
            loopBreakStack.Pop();

            return cnd;
        }

        // Floats in variadic function calls are promoted to doubles, if they are in the trailing arguments
        public static LLVMValueRef GIRFuncCall(ArgonASTFunctionCall call, LLVMModuleRef m, LLVMBuilderRef b)
        {
            var p = new LLVMValueRef[call.parameters.Count];
            int i = 0;
            bool IsVarArg = false;

            IsVarArg = (GetFunctionIsVarArgs(call.FunctionName));

            foreach (var x in call.parameters)
            {
                if (ArgonTypeResolver.ArgonTypeResolver.GetType(x) == "float" && IsVarArg && i > 0)
                {
                    p[i++] = BuildFPCast(b, GIRValueType(x, m, b), DoubleType(), "");
                }
                else
                    p[i++] = GIRValueType(x, m, b);
            }

            return LLVM.BuildCall(b, GetFunctionReference(call.FunctionName), p, "");
        }

        public static LLVMValueRef GIRValueType(ArgonASTModels.ValueTypes.ValueContainer vc, LLVMModuleRef m, LLVMBuilderRef b)
        {
            if (vc is ArgonASTFunctionCall fcall)
                return GIRFuncCall(fcall, m, b);
            if (vc is ArgonASTUnaryOperator un)
                return GIRUnOp(un, m, b);
            if (vc is ArgonASTBinaryOperator op)
                return GIRBinOp(op, m, b);
            if (vc is Terminal t)
                return GetTerminalIR(t, m, b);

            throw new NotImplementedException($"Unknown ValueContainer type {vc.GetType().Name}.");
        }

        public static LLVMValueRef GIRUnOp(ArgonASTUnaryOperator op, LLVMModuleRef m, LLVMBuilderRef b)
        {
            LLVMValueRef left = ConstIntZero, right = ConstIntZero;

            if (op.left is ArgonASTFunctionCall fcalll)
                left = GIRFuncCall(fcalll, m, b);
            else if (op.left is ArgonASTModels.ArgonASTIdentifier idl)
            {
                if (IsVariablePtr(idl.VariableName))
                    left = BuildLoad(b, GetVariableReference(idl.VariableName), "");
                else
                    left = GetTerminalIR(idl, m, b);
            }
            else if (op.left is ArgonASTModels.ValueTypes.Terminal l)
                left = GetTerminalIR(l, m, b);
            else if (op.left is ArgonASTUnaryOperator unn)
                left = GIRUnOp(unn, m, b);
            else if (op.left is ArgonASTModels.ArgonASTBinaryOperator ol)
                left = GIRBinOp(ol, m, b);

            switch (op.Operator)
            {
                case "-":
                    return BuildSub(b, ConstIntZero, left, "");
                case "&":
                    return ArgonSymbolTable.SymbolsTable.GetVariableReference(((ArgonASTIdentifier)op.left).VariableName);
                case "*":
                    return BuildLoad(b, left, ""); 
            }

            throw new NotImplementedException($"Binary Operator type {op.Operator} not implemented for CodeGen.");
        }

        private static Dictionary<(string, string), string> BOPReturns = new Dictionary<(string, string), string>()
        {
            {("int", "int"),        "int" },
            {("float", "int"),      "float" },
            {("int", "float"),      "float" },
            {("float", "float"),    "float" },
            {("string", "string"),  "string" }
        };

        public static LLVMValueRef GIRBinOp(ArgonASTBinaryOperator op, LLVMModuleRef m, LLVMBuilderRef b)
        {
            LLVMValueRef left = ConstIntZero, right = ConstIntZero;

            // Assignment is special
            if (op.Operator == "=")
            {
                var variable = op.left as ArgonASTIdentifier;
                var value = op.right as ValueContainer;

                Console.WriteLine(GetVariableType(variable.VariableName) + " - " + ArgonTypeResolver.ArgonTypeResolver.GetType(value));

                var h = GIRValueType(value, m, b);

                if (GetVariableType(variable.VariableName) != ArgonTypeResolver.ArgonTypeResolver.GetType(value))
                {
                    if (GetVariableType(variable.VariableName) == "float" && ArgonTypeResolver.ArgonTypeResolver.GetType(value) == "int")
                        h = BuildCast(b, LLVMOpcode.LLVMSIToFP, h, FloatType(), "");
                }

                return BuildStore(b, h, GetVariableReference(variable.VariableName));
            }

            if (op.left is ArgonASTFunctionCall fcalll)
                left = GIRFuncCall(fcalll, m, b);
            else if (op.left is ArgonASTModels.ArgonASTIdentifier idl)
            {
                if (IsVariablePtr(idl.VariableName))
                    left = BuildLoad(b, GetVariableReference(idl.VariableName), "");
                else
                    left = GetTerminalIR(idl, m, b);
            }
            else if (op.left is ArgonASTModels.ValueTypes.Terminal l)
                left = GetTerminalIR(l, m, b);
            else if (op.left is ArgonASTUnaryOperator unn)
                left = GIRUnOp(unn, m, b);
            else if (op.left is ArgonASTModels.ArgonASTBinaryOperator ol)
                left = GIRBinOp(ol, m, b);

            if (op.right is ArgonASTFunctionCall fcallr)
                right = GIRFuncCall(fcallr, m, b);
            else if (op.right is ArgonASTModels.ArgonASTIdentifier idr)
                right = BuildLoad(b, GetVariableReference(idr.VariableName), "");
            else if (op.right is ArgonASTModels.ValueTypes.Terminal r)
                right = GetTerminalIR(r, m, b);
            else if (op.right is ArgonASTUnaryOperator unn2)
                right = GIRUnOp(unn2, m, b);
            else if (op.right is ArgonASTModels.ArgonASTBinaryOperator or)
                right = GIRBinOp(or, m, b);

            var leftType = ArgonTypeResolver.ArgonTypeResolver.GetType(op.left);
            var rightType = ArgonTypeResolver.ArgonTypeResolver.GetType(op.right);
            var resultType = BOPReturns[(leftType, rightType)];

            if (resultType == "float")
            {
                left = ArgonTypeCast.ArgonImplicitCasts.ImplicitCastToFloat(left, leftType, b);
                right = ArgonTypeCast.ArgonImplicitCasts.ImplicitCastToFloat(right, rightType, b);
            }

            switch (op.Operator)
            {
                case "+":
                    if (resultType == "int")
                        return BuildAdd(b, left, right, "");
                    else
                        return BuildFAdd(b, left, right, "");

                case "-":
                    if (resultType == "int")
                        return BuildSub(b, left, right, "");
                    else            
                        return BuildFSub(b, left, right, "");

                case "*":
                    if (resultType == "int")
                        return BuildMul(b, left, right, "");
                    else
                        return BuildFMul(b, left, right, "");

                case "/":
                    if (resultType == "int")
                        return BuildSDiv(b, left, right, "");
                    else
                        return BuildFDiv(b, left, right, "");

                case "%":
                    if (resultType == "int")
                        return BuildSRem(b, left, right, "");
                    else
                        return BuildFRem(b, left, right, "");

                case ">":
                    if (resultType == "int")
                        return BuildICmp(b, LLVMIntPredicate.LLVMIntSGT, left, right, "");
                    else
                        return BuildFCmp(b, LLVMRealPredicate.LLVMRealOGT, left, right, "");

                case ">=":
                    if (resultType == "int")
                        return BuildICmp(b, LLVMIntPredicate.LLVMIntSGE, left, right, "");
                    else
                        return BuildFCmp(b, LLVMRealPredicate.LLVMRealOGE, left, right, "");

                case "<":
                    if (resultType == "int")
                        return BuildICmp(b, LLVMIntPredicate.LLVMIntSLT, left, right, "");
                    else
                        return BuildFCmp(b, LLVMRealPredicate.LLVMRealOLT, left, right, "");

                case "<=":
                    if (resultType == "int")
                        return BuildICmp(b, LLVMIntPredicate.LLVMIntSLE, left, right, "");
                    else
                        return BuildFCmp(b, LLVMRealPredicate.LLVMRealOLE, left, right, "");

                case "==":
                    if (resultType == "int")
                        return BuildICmp(b, LLVMIntPredicate.LLVMIntEQ, left, right, "");
                    else
                        return BuildFCmp(b, LLVMRealPredicate.LLVMRealOEQ, left, right, "");

                case "!=":
                    if (resultType == "int")
                        return BuildICmp(b, LLVMIntPredicate.LLVMIntNE, left, right, "");
                    else
                        return BuildFCmp(b, LLVMRealPredicate.LLVMRealONE, left, right, "");
            }

            throw new NotImplementedException($"Binary Operator type {op.Operator} not implemented for CodeGen.");
        }

        private static LLVMValueRef GetTerminalIR(ArgonASTModels.ValueTypes.Terminal t, LLVMModuleRef m, LLVMBuilderRef b)
        {
            switch (t)
            {
                case ArgonASTIntegerLiteral i:
                    return LLVM.ConstInt(Int32Type(), (ulong)i.value, true);
                case ArgonASTFloatLiteral f:
                    return LLVM.ConstReal(FloatType(), f.value);
                case ArgonASTStringLiteral s:
                    return LLVM.BuildGlobalStringPtr(b, s.value, "");
                case ArgonASTIdentifier id:
                    if (IsVariablePtr(id.VariableName))
                        return BuildLoad(b, GetVariableReference(id.VariableName), "");
                    else
                        return GetVariableReference(id.VariableName);
            }

            throw new NotImplementedException($"Literal type {t.GetType().Name} not implemented for CodeGen.");
        }
    }
}
