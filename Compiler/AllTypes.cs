using ExtraLab2018.Nodes;
using ExtraLab2018.Nodes.ClassMembers;
using ExtraLab2018.Nodes.Declarations;
using ExtraLab2018.Nodes.Statements;
using ExtraLab2018.Nodes.Expressions;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace ExtraLab2018.Compiler {
	sealed class AllTypes {
		public readonly ModuleDefinition Module;
		public readonly TypeReference Int;
		public readonly TypeReference Bool;
		public readonly TypeReference Void;
		public readonly TypeReference Object;
		public readonly TypeReference Null;
		public readonly Dictionary<string, TypeReference> types = new Dictionary<string, TypeReference>();
        public readonly Dictionary<string, TypeConstructorDeclaration> typeConstructors = new Dictionary<string, TypeConstructorDeclaration>();

        readonly List<MethodReference> functions = new List<MethodReference>();
		string nullTypeName;
		readonly Dictionary<string, string> myNameBySystemName = new Dictionary<string, string>();
		public AllTypes(ModuleDefinition module) {
			Module = module;
			Int = AddBuiltin("int", module.TypeSystem.Int32);
			Bool = AddBuiltin("bool", module.TypeSystem.Boolean);
			Void = AddBuiltin("void", module.TypeSystem.Void);
			Object = AddBuiltin("object", module.TypeSystem.Object);
			Null = AddBuiltin("Null", null);
			types = new[] {
				Int, Bool, Void, Object
			}.ToDictionary(x => myNameBySystemName[x.FullName]);
			foreach (var m in typeof(BuiltinFunctions).GetMethods()) {
				if (m.IsStatic) {
					functions.Add(module.Import(m));
				}
			}
		}

        public void AddTypeConstructor(TypeConstructorDeclaration c)
        {
            typeConstructors[c.Name] = c;
        }

        private Dictionary<String, IType> Resolve(IType source, IType dest, Dictionary<String, IType> dict)
        {
            foreach (var pair in source.TypeParameters.Zip(dest.TypeParameters, (a, b) => Tuple.Create(a, b)))
            {
                dict[pair.Item1.Name] = pair.Item2;
                pair.Item1.TypeParameters.Zip(pair.Item2.TypeParameters, (a, b) => Resolve(a, b, dict));
            }

            return dict;
        }
     

        public Tuple<ClassDeclaration,Dictionary<String, IType>> ResolveTypeConstructor(IType t)
        {
            if (t.TypeParameters.Count == 0) throw new Exception($"{t.Name} isn't type constructor");
            if (!typeConstructors.ContainsKey(t.Name)) throw new Exception($"Unknown type {t.Name}");
            var type = typeConstructors[t.Name];
            if (type.Constructable(t.TypeParameters))
            {
                var dict = new Dictionary<String, IType>();
                foreach (var pair in type.TypeParameters.Zip(t.TypeParameters, (a, b) => Tuple.Create(a, b)))
                {
                    dict[pair.Item1.Name] = pair.Item2;
                    Resolve(pair.Item1, pair.Item2, dict);
                }
                var newMembers = type.Members.Select<IClassMember, IClassMember>(member =>
                {
                    switch (member)
                    {
                        case ClassField f:
                            var newT = f.Type.Resolve(dict);
                            //if (newT.TypeParameters.Count > 0 && !types.ContainsKey(newT.TypedName))
                            //{
                            //    compile.Invoke(newT);
                            //}
                            return new ClassField(newT, f.Name);
                        case ClassMethod m:
                            var newRetType = m.ReturnType.Resolve(dict);
                            //if (newRetType.TypeParameters.Count > 0 && !types.ContainsKey(newRetType.TypedName))
                            //{
                            //    compile.Invoke(newRetType);
                            //}
                            var pars = m.Parameters.Select(p => {
                                if (p.Type.TypeParameters.Count > 0 && !types.ContainsKey(p.Type.TypedName))
                                {
                                    var newP = p.Type.Resolve(dict);
                                    //if (!types.ContainsKey(newP.TypedName))
                                    //{
                                    //    compile.Invoke(newP);
                                    //}
                                    return new ParameterNode(newP.Resolve(dict), p.Name);

                                }
                                return new ParameterNode(p.Type.Resolve(dict), p.Name);
                                }).ToList().AsReadOnly();
                        
                            IExpression expressionResolve(IExpression exps)
                            {
                                switch(exps)
                                {
                                    case TypedIdentifier i:
                                        return i.Resolve(dict);
                                        return new TypedIdentifier(i.Name, i.TypeParameters.Select(t1 => t1.Resolve(dict)).ToList());
                                    case CallExpression e:
                                        return new CallExpression(expressionResolve(e.Function), e.Arguments.Select(expressionResolve).ToList());
                                }
                                return exps;
                            }

                            var newBody = m.Body.Statements.Select(st =>
                            {
                                
                                switch (st)
                                {
                                    case Assignment a:
                                        
                                        //if (a.Type != null && a.Type.TypeParameters.Count > 0 && !types.ContainsKey(a.Type.TypedName))
                                        //{
                                        //    compile.Invoke(a.Type);
                                        //}
                                        return new Assignment(a.Target, a.Type?.Resolve(dict), expressionResolve(a.Expr));
                                    case Return r:
                                        return new Return(expressionResolve(r.Expr));

                                    default: return st;
                                }
                            });
                            return new ClassMethod(newRetType, m.Name, pars, new Block(newBody.ToList().AsReadOnly()));
                    }
                    throw new Exception("wtf");
                });
                var newClass = new ClassDeclaration(t.TypedName, newMembers.ToList().AsReadOnly());
                return Tuple.Create(newClass,dict);

            }
            else
            {
                throw new Exception($"Type constructor {t.TypedName} doesn't exist");
            }
        }

		TypeReference AddBuiltin(string myName, TypeReference tr) {
			if (tr == null) {
				Debug.Assert(nullTypeName == null);
				nullTypeName = myName;
			}
			else {
				myNameBySystemName.Add(tr.FullName, myName);
			}
			types.Add(myName, tr);
			return tr;
		}
		public string GetTypeName(TypeReference tr) {
			if (tr == null) {
				return nullTypeName;
			}
			return myNameBySystemName[tr.FullName];
		}
		public void AddFunction(MethodDefinition md) {
			functions.Add(md);
		}
		public void AddType(TypeDefinition td) {
            if (myNameBySystemName.ContainsKey(td.FullName)) return;

            myNameBySystemName.Add(td.FullName, td.FullName);
			Module.Types.Add(td);
			types.Add(td.Name, td);
		}
		public TypeReference GetMyType(string name) {
			var tr = TryGetMyType(name);
			if (tr == null) {
				throw new Exception($"Неизвестный тип {name}");
			}
			return tr;
		}
		public TypeReference TryGetMyType(string name) {
			TypeReference tr;
			if (types.TryGetValue(name, out tr)) {
				return tr;
            }
            else
            {
                int a = 2;
            }
			return null;
		}
		public bool CanCall(MethodReference mr, IReadOnlyList<TypeReference> argumentTypes) {
			return mr.Parameters
				.Select(p => p.ParameterType)
				.SequenceEqual(argumentTypes, TypeReferenceEqualityComparer.Instance);
		}
		public MethodReference TryGetFunction(string functionName, IReadOnlyList<TypeReference> argumentTypes) {
			return functions
				.Where(fn => fn.Name == functionName && CanCall(fn, argumentTypes))
				.SingleOrDefault();
		}
		public MethodReference TryGetMethod(TypeReference tr, string methodName, IReadOnlyList<TypeReference> argumentTypes) {
			var mr = tr.Resolve().Methods
				.Where(fn => fn.Name == methodName && CanCall(fn, argumentTypes))
				.SingleOrDefault();
			if (mr == null) {
				return null;
			}
			return Module.Import(mr);
		}
	}
}
