using System;
using System.Collections.Generic;
using System.Text;

using LLVMSharp;
using static LLVMSharp.LLVM;
using ArgonASTModels;
using ArgonASTModels.ValueTypes;

using static ArgonCodeGen.SymbolsTable;

namespace ArgonCodeGen
{
    public static class ArgonCodeGen
    {
        private static LLVMValueRef ConstIntZero = LLVM.ConstInt(Int32Type(), 0, true);

        private static LLVMValueRef thisFuncRetVal = ConstIntZero;
        private static LLVMBasicBlockRef thisFuncRetBlock;

        public static void GetGeneratedCode(ArgonASTBase arg)
        {
            var context = ContextCreate();
            var module = LLVM.ModuleCreateWithNameInContext("top", context);
            var builder = LLVM.CreateBuilderInContext(context);

            var block = arg as ArgonASTBlock;

            // Printf
            var printfPType = new LLVMTypeRef[1] { PointerType(Int8Type(), 0) };
            var printfFType = FunctionType(Int32Type(), printfPType, true);
            var printfFunction = LLVM.AddFunction(module, "printf", printfFType);

            SymbolsTable.Functions.Add("printf", ("int", printfFunction));

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

                        SymbolsTable.Functions.Add(f.FunctionName, (f.ReturnType, fn));
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

        private static void GIRBlock(ArgonASTBlock block, LLVMModuleRef m, LLVMBuilderRef b, LLVMValueRef fn,LLVMBasicBlockRef thisBlock)
        {
            if (block == null) return;
            foreach (var t in block.Children)
            {
                switch (t)
                {
                    case ArgonASTDeclaration decl:
                        var vb = BuildAlloca(b, AST2LLVMTypes[decl.Type], "");
                        Variables.Add(decl.VariableName, (decl.Type, vb, true));
                        break;

                    case ArgonASTAssignment ass:

                        switch (Variables[ass.variable].type)
                        {
                            case "int":
                                var h = GIRValueType(ass.value, m, b);
                                BuildStore(b, h, Variables[ass.variable].vref);
                                break;

                            case "string":
                                var hs = GIRValueType(ass.value, m, b);
                                BuildStore(b, hs, Variables[ass.variable].vref);
                                break;
                        }

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

                    case ArgonASTReturn ret:
                        var rp = ret.expression;
                        //BuildRet(b, GIRValueType(rp, m, b));
                        BuildStore(b, GIRValueType(rp, m, b), thisFuncRetVal);
                        BuildBr(b, thisFuncRetBlock);
                        break;
                }
            }
        }

        private static void GIRFuncDecl(ArgonASTFunctionDeclaration f, LLVMModuleRef m, LLVMBuilderRef b)
        {
            LLVMValueRef returnSt = ConstIntZero, retValue = ConstIntZero;

            var bb = AppendBasicBlock(SymbolsTable.Functions[f.FunctionName].vref, "");
            var endbb = AppendBasicBlock(SymbolsTable.Functions[f.FunctionName].vref, "");

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

            var parameters = SymbolsTable.Functions[f.FunctionName].vref.GetParams();
            int ix = 0;
            foreach (var x in parameters)
                SymbolsTable.Variables.Add(f.FormalParamaters[ix].VariableName, (f.FormalParamaters[ix++].Type, x, false));


            GIRBlock(f.FunctionBody, m, b, SymbolsTable.Functions[f.FunctionName].vref, bb);

            var sxx = GetLastInstruction(GetLastBasicBlock(SymbolsTable.Functions[f.FunctionName].vref).GetPreviousBasicBlock());
            if (!LLVM.IsABranchInst(sxx).ToString().Trim().StartsWith("br"))
            {
                PositionBuilderAtEnd(b, GetLastBasicBlock(SymbolsTable.Functions[f.FunctionName].vref).GetPreviousBasicBlock());
                BuildBr(b, endbb);
            }

            ////var list = GetBasicBlocks(SymbolsTable.Functions[f.FunctionName].vref);
            //PositionBuilderAtEnd(b, GetLastBasicBlock(SymbolsTable.Functions[f.FunctionName].vref));
            //BuildBr(b, thisFuncRetBlock);
        }

        public static void GIRIfElse(ArgonASTIf iff, LLVMModuleRef m, LLVMBuilderRef b, LLVMValueRef fn, 
            LLVMBasicBlockRef thisBlock)
        {
            var doesTrueReturn = ArgonBranchAnalyzer.ArgonAnalyzerBranchReturn.DoesBranchReturnFromAllPaths(iff.trueBlock);
            var doesFalseReturn = ArgonBranchAnalyzer.ArgonAnalyzerBranchReturn.DoesBranchReturnFromAllPaths(iff.falseBlock);

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

            //bool makeMergeBlock = true;

            //var cnd = GIRValueType(iff.condition, m, b);

            //var thenBlock = thisBlock.InsertBasicBlock("then");
            //thenBlock.MoveBasicBlockAfter(thisBlock);

            //LLVMBasicBlockRef elseBlock, mergeBlock = new LLVMBasicBlockRef();

            //if ((doesTrueReturn && doesFalseReturn) || (doesTrueReturn && (iff.falseBlock == null)))
            //    makeMergeBlock = false;

            //PositionBuilderAtEnd(b, thenBlock);
            //GIRBlock(iff.trueBlock, m, b, fn, thenBlock);

            //if (!doesTrueReturn)
            //{
            //    BuildBr(b, mergeBlock);
            //}



            //LLVMBasicBlockRef nextBlock = thisBlock.GetNextBasicBlock();
            //var cnd = GIRValueType(iff.condition, m, b);

            //var thenBlock = thisBlock.InsertBasicBlock("then");
            //var mergeBlock = thisBlock.InsertBasicBlock("merge");
            //var elseBlock = iff.falseBlock != null ? thisBlock.InsertBasicBlock("else") : mergeBlock;

            //thenBlock.MoveBasicBlockAfter(thisBlock);
            //if (iff.falseBlock != null)
            //    elseBlock.MoveBasicBlockAfter(thenBlock);
            //mergeBlock.MoveBasicBlockAfter(iff.falseBlock != null ? elseBlock : thenBlock);

            //BuildCondBr(b, cnd, thenBlock, elseBlock);

            //PositionBuilderAtEnd(b, thenBlock);
            //GIRBlock(iff.trueBlock, m, b, fn, thenBlock);

            //var sxx = GetLastInstruction(thenBlock);
            //if (!LLVM.IsABranchInst(sxx).ToString().Trim().StartsWith("br"))
            //{
            //    BuildBr(b, mergeBlock);
            //}

            //if (iff.falseBlock != null)
            //{
            //    PositionBuilderAtEnd(b, elseBlock);
            //    GIRBlock(iff.falseBlock, m, b, fn, elseBlock);

            //    var syy = GetLastInstruction(elseBlock);
            //    Console.WriteLine(syy);
            //    if (!syy.ToString().Trim().StartsWith("br"))
            //    {
            //        BuildBr(b, mergeBlock);
            //    }
            //}

            //PositionBuilderAtEnd(b, mergeBlock);

            //return cnd;
        }

        public static LLVMValueRef GIRWhile(ArgonASTWhile whl, LLVMModuleRef m, LLVMBuilderRef b, LLVMValueRef fn, 
            LLVMBasicBlockRef thisBlock)
        {
            LLVMBasicBlockRef nextBlock = thisBlock.GetNextBasicBlock();

            var entryBlock = thisBlock.InsertBasicBlock("");
            var loopBlock = thisBlock.InsertBasicBlock("");
            var exitBlock = thisBlock.InsertBasicBlock("");

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

            return cnd;
        }

        public static LLVMValueRef GIRFuncCall(ArgonASTFunctionCall call, LLVMModuleRef m, LLVMBuilderRef b)
        {
            var p = new LLVMValueRef[call.parameters.Count];
            int i = 0;
            foreach (var x in call.parameters)
            {
                //// Load the identifier before using it
                //if (x is ArgonASTIdentifier str)
                //    p[i++] = BuildLoad(b, GIRValueType(x, m, b), "");
                //else
                p[i++] = GIRValueType(x, m, b);
            }

            return LLVM.BuildCall(b, SymbolsTable.Functions[call.FunctionName].vref, p, "");
        }

        public static LLVMValueRef GIRValueType(ArgonASTModels.ValueTypes.ValueContainer vc, LLVMModuleRef m, LLVMBuilderRef b)
        {
            if (vc is ArgonASTFunctionCall fcall)
                return GIRFuncCall(fcall, m, b);
            if (vc is ArgonASTBinaryOperator op)
                return GIRBinOp(op, m, b);
            if (vc is Terminal t)
                return GetTerminalIR(t, m, b);

            throw new NotImplementedException($"Unknown ValueContainer type {vc.GetType().Name}.");
        }

        public static LLVMValueRef GIRBinOp(ArgonASTBinaryOperator op, LLVMModuleRef m, LLVMBuilderRef b)
        {
            LLVMValueRef left = ConstIntZero, right = ConstIntZero;

            if (op.left is ArgonASTFunctionCall fcalll)
                left = GIRFuncCall(fcalll, m, b);
            else if (op.left is ArgonASTModels.ArgonASTIdentifier idl)
            {
                Console.WriteLine(SymbolsTable.Variables.ContainsKey(idl.VariableName));
                if (SymbolsTable.Variables[idl.VariableName].IsPtr)
                    left = BuildLoad(b, SymbolsTable.Variables[idl.VariableName].vref, "");
                else
                    left = GetTerminalIR(idl, m, b);
            }
            else if (op.left is ArgonASTModels.ValueTypes.Terminal l)
                left = GetTerminalIR(l, m, b);
            else if (op.left is ArgonASTModels.ArgonASTBinaryOperator ol)
                left = GIRBinOp(ol, m, b);

            if (op.right is ArgonASTFunctionCall fcallr)
                right = GIRFuncCall(fcallr, m, b);
            else if (op.right is ArgonASTModels.ArgonASTIdentifier idr)
                right = BuildLoad(b, SymbolsTable.Variables[idr.VariableName].vref, "");
            else if (op.right is ArgonASTModels.ValueTypes.Terminal r)
                right = GetTerminalIR(r, m, b);
            else if (op.right is ArgonASTModels.ArgonASTBinaryOperator or)
                right = GIRBinOp(or, m, b);

            switch (op.Operator)
            {
                case "+":
                    return BuildAdd(b, left, right, "");
                case "-":
                    return BuildSub(b, left, right, "");
                case "*":
                    return BuildMul(b, left, right, "");
                case "/":
                    return BuildSDiv(b, left, right, "");
                case "%":
                    return BuildSRem(b, left, right, "");

                case ">":
                    return BuildICmp(b, LLVMIntPredicate.LLVMIntSGT, left, right, "");
                case ">=":
                    return BuildICmp(b, LLVMIntPredicate.LLVMIntSGE, left, right, "");
                case "<":
                    return BuildICmp(b, LLVMIntPredicate.LLVMIntSLT, left, right, "");
                case "<=":
                    return BuildICmp(b, LLVMIntPredicate.LLVMIntSLE, left, right, "");
                case "==":
                    return BuildICmp(b, LLVMIntPredicate.LLVMIntEQ, left, right, "");
                case "!=":
                    return BuildICmp(b, LLVMIntPredicate.LLVMIntNE, left, right, "");
            }

            throw new NotImplementedException($"Binary Operator type {op.Operator} not implemented for CodeGen.");
        }

        private static LLVMValueRef GetTerminalIR(ArgonASTModels.ValueTypes.Terminal t, LLVMModuleRef m, LLVMBuilderRef b)
        {
            switch (t)
            {
                case ArgonASTIntegerLiteral i:
                    return LLVM.ConstInt(Int32Type(), (ulong)i.value, true);
                case ArgonASTStringLiteral s:
                    return LLVM.BuildGlobalStringPtr(b, s.value, "");
                case ArgonASTIdentifier id:
                    if (SymbolsTable.Variables[id.VariableName].IsPtr)
                        return BuildLoad(b, SymbolsTable.Variables[id.VariableName].vref, "");
                    else
                        return SymbolsTable.Variables[id.VariableName].vref;
            }

            throw new NotImplementedException($"Literal type {t.GetType().Name} not implemented for CodeGen.");
        }
    }
}
