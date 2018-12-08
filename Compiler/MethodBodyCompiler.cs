using ExtraLab2018.Nodes;
using ExtraLab2018.Nodes.Expressions;
using ExtraLab2018.Nodes.Statements;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using System.Linq;
namespace ExtraLab2018.Compiler
{
    sealed class MethodBodyCompiler : IStatementVisitor, IExpressionVisitor<TypeReference>
    {
        readonly AllTypes types;
        readonly MethodDefinition md;
        readonly ILProcessor asm;
        readonly Dictionary<String, int> vars = new Dictionary<string, int>();
        readonly Stack<Instruction> loopEnds = new Stack<Instruction>();
       // readonly Dictionary<String, IType> typeParameters;

        //TypedIdentifier ResolveTypeParameters(TypedIdentifier ident)
        //{
        //    var type = new IType(ident.NonTypedName, ident.TypeParameters);
        //    if (types.TryGetMyType(type.TypedName) != null) return ident;
        //    if (type.Resolveable(typeParameters))
        //    {
        //        var newType = type.Resolve(typeParameters);
        //        return new TypedIdentifier(newType.Name, newType.TypeParameters);
        //    }
        //    return ident;
        //}

        public MethodBodyCompiler(AllTypes types, MethodDefinition md, Dictionary<String, IType> pars)
        {
            this.types = types;
            this.md = md;
            asm = md.Body.GetILProcessor();
           // typeParameters = pars;
        }
        public void CompileMethodStatements(IEnumerable<IStatement> statements)
        {
            foreach (var s in statements) s.Accept(this);
            asm.Emit(OpCodes.Ret);
        }
        Exception Error(string message)
        {
            return new Exception(message);
        }
        Exception WrongType(IExpression expression, TypeReference actual, TypeReference expected)
        {
            return Error($"Выражение {expression.FormattedString} имеет тип {types.GetTypeName(actual)} вместо {types.GetTypeName(expected)}");
        }
        #region statements
        private void CompileStatement(IStatement statement)
        {
            statement.Accept(this);
        }

        void IStatementVisitor.VisitIf(IfStatement statement)
        {
            var nop = Instruction.Create(OpCodes.Nop);
            var expr = CompileExpression(statement.Condition);

            if (expr.EqualTo(types.Bool))
            {
                asm.Emit(OpCodes.Brfalse, nop);
                foreach (var s in statement.Body.Statements) CompileStatement(s);
                asm.Append(nop);
            }
            else
            {
                throw new Exception($"Wrong type {expr}");
            }
        }
        void IStatementVisitor.VisitWhile(WhileStatement statement)
        {

            var conditionStart = Instruction.Create(OpCodes.Nop);
            asm.Append(conditionStart);
            var cond = CompileExpression(statement.Condition);

            if (cond.EqualTo(types.Bool))
            {
                var bodyStart = Instruction.Create(OpCodes.Nop);
                var bodyEnd = Instruction.Create(OpCodes.Nop);
                asm.Emit(OpCodes.Brfalse, bodyEnd);
                asm.Append(bodyStart);
                loopEnds.Push(bodyEnd);
                foreach (var s in statement.Body.Statements) CompileStatement(s);
                loopEnds.Pop();
                asm.Emit(OpCodes.Br, conditionStart);
                asm.Append(bodyEnd);
            }
            else
            {
                throw Error($"Wrong type {cond}");
            }
        }

        void IStatementVisitor.VisitExpressionStatement(ExpressionStatement statement)
        {
            var retType = CompileExpression(statement.Expr);
            if (!retType.EqualTo(types.Void))
            {
                asm.Emit(OpCodes.Pop);
            }
        }


        void IStatementVisitor.VisitAssignment(Assignment statement)
        {
            
            var expr = CompileExpression(statement.Expr);

            switch (statement.Target)
            {
                case Identifier i when vars.ContainsKey(i.Name):
                    var index = vars[i.Name];

                    var v = md.Body.Variables.Single(variable => variable.Index == index);
                    if (v.VariableType.EqualTo(expr))
                    {
                        asm.Emit(OpCodes.Stloc, v);
                        return;
                    }
                    throw Error("Wrong type ");
                case Identifier i when md.Parameters.FirstOrDefault(p => p.Name == i.Name) != null:
                    var p1 = md.Parameters.First(p => p.Name == i.Name);
                    asm.Emit(OpCodes.Starg, p1);
                    return;
                case Identifier i:
                    var newVar = new VariableDefinition(expr);
                    md.Body.Variables.Add(newVar);
                    vars[i.Name] = newVar.Index;
                    asm.Emit(OpCodes.Stloc, newVar);
                    return;

                case MemberAccess m:
                    var obj = CompileExpression(m.Obj);
                    var field = obj.Resolve().Fields.FirstOrDefault(v1 => v1.Name == m.Member);
                    if (field != null && field.FieldType.EqualTo(expr))
                    {
                        asm.Emit(OpCodes.Stfld, field);
                        return;
                    }
                    throw Error("Field doesn't exist");
            }
        }


        void IStatementVisitor.VisitBreak(Break statement)
        {
            md.Body.GetILProcessor().Emit(OpCodes.Br, loopEnds.Peek());
        }
        void IStatementVisitor.VisitReturn(Return statement)
        {
            var res = CompileExpression(statement.Expr);
            if (!res.EqualTo(types.Void))
            {
                asm.Emit(OpCodes.Ret);
            }
            else
            {
                throw new Exception($"$WrongType {statement.FormattedString}");
            }
        }
        #endregion
        #region expressions
        private TypeReference CompileExpression(IExpression expression)
        {
            return expression.Accept(this);
        }
        TypeReference IExpressionVisitor<TypeReference>.VisitBinary(BinaryOperation expression)
        {
            var left = CompileExpression(expression.Left);

            var right = CompileExpression(expression.Right);

            switch (expression.Operation)
            {
                case BinaryOpType.Addition when left.EqualTo(types.Int) && right.EqualTo(types.Int):
                    asm.Emit(OpCodes.Add);
                    return types.Int;
                case BinaryOpType.Subtraction when left.EqualTo(types.Int) && right.EqualTo(types.Int):
                    asm.Emit(OpCodes.Sub);
                    return types.Int;
                case BinaryOpType.Multiplication when left.EqualTo(types.Int) && right.EqualTo(types.Int):
                    asm.Emit(OpCodes.Mul);
                    return types.Int;

                case BinaryOpType.Equal when left.EqualTo(right):
                    asm.Emit(OpCodes.Ceq);
                    return types.Bool;
              

                case BinaryOpType.Less: //when left == types.Int && right == types.Int:
                    asm.Emit(OpCodes.Clt);
                    return types.Bool;
            }

            throw new NotImplementedException();
        }
        TypeReference IExpressionVisitor<TypeReference>.VisitConditional(ConditionalExpression expression)
        {
            throw new Exception();
        }
        TypeReference IExpressionVisitor<TypeReference>.VisitUnary(UnaryOperation expression)
        {         
            throw Error($"wrong {expression.Operation}");
        }

        TypeReference IExpressionVisitor<TypeReference>.VisitCall(CallExpression expression)
        {
            switch (expression.Function)
            {
                case Identifier i when types.TryGetMyType(i.Name) != null || i is TypedIdentifier:
                    //if (i is TypedIdentifier && types.TryGetMyType(i.Name) == null)
                    //{
                    //    var typed = i as TypedIdentifier;
                    //    var type1 = new IType(typed.NonTypedName, typed.TypeParameters);
                    //    try
                    //    {
                    //        compile(type1);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        types.types.Remove(type1.TypedName);
                    //        typed = ResolveTypeParameters(typed);
                    //        i = typed;
                    //        if (types.TryGetMyType(typed.Name) == null)
                    //        {
                    //            compile(new IType(typed.NonTypedName, typed.TypeParameters));
                    //        }
                    //    }
                    //}
                    var t = types.TryGetMyType(i.Name);
                    var c = t.Resolve().Methods.FirstOrDefault(m1 => m1.Name == ".ctor");
                    if (c != null)
                    {
                        var e = expression.Arguments.Select(CompileExpression).ToList();

                        if (types.CanCall(c, e))
                        {
                            asm.Emit(OpCodes.Newobj, c);
                            return t;
                        }
                    }
                    else
                    {

                    }
                    throw Error("Wrong type argument");
                case Identifier i:

                    var e1 = expression.Arguments.Select(CompileExpression);

                    IReadOnlyList<TypeReference> args = e1.ToList().AsReadOnly();
                    var m = types.TryGetFunction(i.Name, args);
                    if (m != null)
                    {
                        asm.Emit(OpCodes.Call, m);
                        return m.ReturnType;
                    }
                    throw Error("Wrong type argument");
                case MemberAccess m4:
                    var type = CompileExpression(m4.Obj);
                    var e2 = expression.Arguments.Select(CompileExpression).ToList();
                    var m3 = types.TryGetMethod(type, m4.Member, e2);
                    if (m3 != null)
                    {

                        asm.Emit(OpCodes.Callvirt, m3);
                        return m3.ReturnType;
                    }
                    throw Error("Wrong type argument");
            }

            throw Error("Wrong type argument");
        }

        TypeReference IExpressionVisitor<TypeReference>.VisitParen(Paren expression)
        {
            var expr = CompileExpression(expression.Expr);
            if (!expr.EqualTo(types.Void))
            {
                return expr;
            }
            throw new Exception($"wrong type {expr}");
        }
        TypeReference IExpressionVisitor<TypeReference>.VisitNumber(Number expression)
        {
            if (int.TryParse(expression.Lexeme, out int res))
            {
                asm.Emit(OpCodes.Ldc_I4, res);
                return types.Int;
            }
            throw new Exception($"Wrong lexeme {expression.Lexeme}");
        }
        TypeReference IExpressionVisitor<TypeReference>.VisitIdentifier(Identifier expression)
        {

            switch (expression.Name)
            {
                case "true":
                    asm.Emit(OpCodes.Ldc_I4, 1);
                    return types.Bool;
                case "false":
                    asm.Emit(OpCodes.Ldc_I4, 0);
                    return types.Bool;
                case "this":
                    asm.Emit(OpCodes.Ldarg, md.Body.ThisParameter);
                    return md.Body.ThisParameter.ParameterType;
            }
            if (vars.ContainsKey(expression.Name))
            {
                var ind = vars[expression.Name];
                var variable = md.Body.Variables.First(v => v.Index == ind);
                asm.Emit(OpCodes.Ldloc, variable);
                return variable.VariableType;
            }
            else
            {
                switch (md.Parameters.FirstOrDefault(p => p.Name == expression.Name))
                {
                    case ParameterDefinition p:
                        asm.Emit(OpCodes.Ldarg, p);
                        return p.ParameterType;
                }
            }
            throw Error($"Wrong lexeme {expression.FormattedString}");
        }

        TypeReference IExpressionVisitor<TypeReference>.VisitMemberAccess(MemberAccess expression)
        {
            var expr = CompileExpression(expression.Obj);
            if (!expr.EqualTo(types.Void))
            {
                var member = expr.Resolve().Fields.FirstOrDefault(x => x.Name == expression.Member);
                if (member != null)
                {
                    asm.Emit(OpCodes.Ldfld, member);
                    return member.FieldType;
                }
            }
            throw Error($"wrong type {expr}");
        }
        TypeReference IExpressionVisitor<TypeReference>.VisitTypedExpression(TypedExpression expression)
        {
            var expr = CompileExpression(expression.Expr);
            var type = types.GetMyType(expression.Type);
            if (expr.EqualTo(type))
                return type;
            else
                throw Error($"Wrong type {expression.FormattedString}");
        }
        #endregion
    }
}
