using ExtraLab2018.Nodes;
using ExtraLab2018.Nodes.ClassMembers;
using ExtraLab2018.Nodes.Declarations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ExtraLab2018.Compiler {
	sealed class ProgramCompiler {
		readonly AllTypes types;
		TypeDefinition staticClass;
		MethodDefinition mainMethod;
		readonly Dictionary<ClassMethod, MethodDefinition> classMethodDefinitionByNode = new Dictionary<ClassMethod, MethodDefinition>();
		readonly Dictionary<FunctionDeclaration, MethodDefinition> functionMethodDefinitionByNode = new Dictionary<FunctionDeclaration, MethodDefinition>();
		public ProgramCompiler(AllTypes allTypes) {
			types = allTypes;
		}
		public void Compile(ProgramNode programNode, string staticClassName = "Entry", string mainMethodName = "Main") {
            
			AddStaticClass(staticClassName);
			AddMainMethod(mainMethodName);
			AddTypes(programNode);
            var r = AddTypeCons(programNode);
            AddTypeMembers(programNode,r);
			AddFunctions(programNode);
			CompileFunctions(programNode);
			CompileClassMethods(programNode,r);
			CompileMainMethod(programNode);
			types.Module.EntryPoint = mainMethod;
		}
		void AddStaticClass(string staticClassName) {
			var td = new TypeDefinition(
				"", staticClassName,
				TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
				types.Module.TypeSystem.Object
				);
			staticClass = td;
			types.Module.Types.Add(td);
		}
		void AddMainMethod(string mainMethodName) {
			var main = new MethodDefinition(
				"Main",
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
				types.Module.TypeSystem.Void
				);
			staticClass.Methods.Add(main);
			mainMethod = main;
			types.Module.EntryPoint = main;
		}
		void AddTypes(ProgramNode programNode) {
			foreach (var classNode in programNode.Declarations.OfType<ClassDeclaration>()) {
                if (classNode is TypeConstructorDeclaration)
                {
                    types.AddTypeConstructor(classNode as TypeConstructorDeclaration);
                }
                else
                {
                    var td = createFromClassNode(classNode);
                    types.AddType(td);
                }
			}
		}


        void ResolveTypeCons(List<IType> l,List<ClassDeclaration> res)
        {
            foreach (var classNode in l)
            {
                if (classNode.TypeParameters.Count > 0)
                {
                    var t = classNode.Constructable(types);
                    if (types.typeConstructors.ContainsKey(classNode.Name) &&
                       !types.types.ContainsKey(classNode.TypedName) &&
                       t.Item1)
                    {
                        ResolveTypeCons(t.Item2, res);
                        var tuple = types.ResolveTypeConstructor(classNode);
                        var td = createFromClassNode(tuple.Item1);
                        res.Add(tuple.Item1);
                        types.AddType(td);
                    }
                }
            }
        }

        List<ClassDeclaration> AddTypeCons(ProgramNode programNode)
        {
            var res = new List<ClassDeclaration>();
            ResolveTypeCons(programNode.TypeCons.ToList(), res);
            return res;
        }

        private TypeDefinition createFromClassNode(ClassDeclaration classNode)
        => new TypeDefinition(
                        "", classNode.Name,
                        TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                        types.Module.TypeSystem.Object
                        );

        private TypeDefinition createFromClassName(String name)
        => new TypeDefinition(
                        "", name,
                        TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                        types.Module.TypeSystem.Object
                        );


        void AddTypeMembers(ProgramNode programNode,List<ClassDeclaration> typeCons) {
			foreach (var classNode in programNode.Declarations.OfType<ClassDeclaration>().Concat(typeCons)) {
                if (classNode is TypeConstructorDeclaration) continue;
				var td = types.GetMyType(classNode.Name).Resolve();
				foreach (var fieldNode in classNode.Members.OfType<ClassField>()) {
                    //if (fieldNode.Type.TypeParameters.Count > 0 && types.TryGetMyType(fieldNode.Type.TypedName) == null)
                    //{
                    //    CompileConstructor(fieldNode.Type);
                    //}

                    var fd = new FieldDefinition(
						fieldNode.Name,
						FieldAttributes.Public,
						types.GetMyType(fieldNode.Type.TypedName)
						);
					td.Fields.Add(fd);
				}
				foreach (var methodNode in classNode.Members.OfType<ClassMethod>()) {
                    //if (methodNode.ReturnType.TypeParameters.Count > 0 && types.GetMyType(methodNode.ReturnType.TypedName) == null)
                    //{
                    //    CompileConstructor(methodNode.ReturnType);
                    //}


					var md = new MethodDefinition(
						methodNode.Name,
						MethodAttributes.Public | MethodAttributes.HideBySig,
						types.GetMyType(methodNode.ReturnType.TypedName)
						);
					td.Methods.Add(md);
					classMethodDefinitionByNode.Add(methodNode, md);
					AddParameters(md, methodNode.Parameters);
				}
			}
		}
		void AddFunctions(ProgramNode programNode) {
			foreach (var functionNode in programNode.Declarations.OfType<FunctionDeclaration>()) {
                //if (functionNode.ReturnType.TypeParameters.Count > 0 && types.TryGetMyType(functionNode.ReturnType.TypedName) == null)
                //{
                //    CompileConstructor(functionNode.ReturnType);
                //}
                var md = new MethodDefinition(
					functionNode.Name,
					MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
					types.GetMyType(functionNode.ReturnType.TypedName)
					);
				staticClass.Methods.Add(md);
				types.AddFunction(md);
				functionMethodDefinitionByNode.Add(functionNode, md);
				AddParameters(md, functionNode.Parameters);
			}
		}
		void AddParameters(MethodDefinition md, IReadOnlyList<ParameterNode> Parameters) {
			foreach (var parameterNode in Parameters) {
                //if (parameterNode.Type.TypeParameters.Count > 0 && types.TryGetMyType(parameterNode.Type.TypedName) == null)
                //{
                //    CompileConstructor(parameterNode.Type);
                //}
                var parameter = new ParameterDefinition(
					parameterNode.Name,
					ParameterAttributes.None,
					types.GetMyType(parameterNode.Type.TypedName)
					);
				md.Parameters.Add(parameter);
			}
		}
		void CompileFunctions(ProgramNode programNode) {
			foreach (var functionNode in programNode.Declarations.OfType<FunctionDeclaration>()) {
				CompileFunction(functionNode);
			}
		}
		void CompileClassMethods(ProgramNode programNode, List<ClassDeclaration> typeCons) {
			foreach (var classNode in programNode.Declarations.OfType<ClassDeclaration>().Concat(typeCons)) {
                if (classNode is TypeConstructorDeclaration) continue;
                CompileClassMethods(classNode);
			}
		}

        void CompileClassMethods(ClassDeclaration classNode)
        {
            var td = types.GetMyType(classNode.Name).Resolve();
            CompileClassConstructor(td);
            foreach (var classMethod in classNode.Members.OfType<ClassMethod>())
            {
                CompileClassMethod(classMethod);
            }
        }


     

        void CompileClassConstructor(TypeDefinition type) {
			var ctor = new MethodDefinition(
				".ctor",
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
				types.Module.TypeSystem.Void
				);
			type.Methods.Add(ctor);
			var asm = ctor.Body.GetILProcessor();
			asm.Emit(OpCodes.Ldarg, 0);
			asm.Emit(OpCodes.Call, types.Module.Import(typeof(object).GetConstructor(new Type[] { })));
			foreach (var field in type.Fields) {
				var pd = new ParameterDefinition(field.Name, ParameterAttributes.None, field.FieldType);
				ctor.Parameters.Add(pd);
				asm.Emit(OpCodes.Ldarg, ctor.Body.ThisParameter);
				asm.Emit(OpCodes.Ldarg, pd);
				asm.Emit(OpCodes.Stfld, field);
			}
			asm.Emit(OpCodes.Ret);
		}
		void CompileFunction(FunctionDeclaration functionNode) {
			var md = functionMethodDefinitionByNode[functionNode];
			new MethodBodyCompiler(
				types, md
				,new Dictionary<string, IType>()).CompileMethodStatements(functionNode.Body.Statements);
		}
		void CompileClassMethod(ClassMethod classMethod) {
			var md = classMethodDefinitionByNode[classMethod];
			new MethodBodyCompiler(
				types, md
				, new Dictionary<string, IType>()).CompileMethodStatements(classMethod.Body.Statements);
		}
		void CompileMainMethod(ProgramNode programNode) {
			new MethodBodyCompiler(
				types, mainMethod
				, new Dictionary<string, IType>()).CompileMethodStatements(programNode.Statements);
		}

                //void CompileConstructor(IType type)
                //{
                //    var t2 = createFromClassName(type.TypedName);
                //    types.AddType(t2);
                //    var tuple = types.ResolveTypeConstructor(type);
                //    var classNode = tuple.Item1;
                //    var dict = tuple.Item2;
                //    var td = t2.Resolve();
                //    foreach (var fieldNode in classNode.Members.OfType<ClassField>())
                //    {
                //        if (fieldNode.Type.TypeParameters.Count > 0 && types.TryGetMyType(fieldNode.Type.TypedName) == null)
                //        {
                //            CompileConstructor(fieldNode.Type);
                //        }

                //        var fd = new FieldDefinition(
                //            fieldNode.Name,
                //            FieldAttributes.Public,
                //            types.GetMyType(fieldNode.Type.TypedName)
                //            );
                //        td.Fields.Add(fd);
                //    }
                //    foreach (var methodNode in classNode.Members.OfType<ClassMethod>())
                //    {
                //        if (methodNode.ReturnType.TypeParameters.Count > 0 && types.TryGetMyType(methodNode.ReturnType.TypedName) == null)
                //        {
                //            CompileConstructor(methodNode.ReturnType);
                //        }

                //        var md = new MethodDefinition(
                //            methodNode.Name,
                //            MethodAttributes.Public | MethodAttributes.HideBySig,
                //            types.GetMyType(methodNode.ReturnType.TypedName)
                //            );
                //        td.Methods.Add(md);
                //        classMethodDefinitionByNode.Add(methodNode, md);
                //        AddParameters(md, methodNode.Parameters);

               
            
                //    }
                //    CompileClassConstructor(td);
                //    foreach (var classMethod in classNode.Members.OfType<ClassMethod>())
                //    {
                //        var md = classMethodDefinitionByNode[classMethod];
                //        new MethodBodyCompiler(
                //            types, md
                //            , CompileConstructor, dict).CompileMethodStatements(classMethod.Body.Statements);
                //    }
                //}

       
	}
}
