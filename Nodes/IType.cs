using ExtraLab2018.Compiler;
using ExtraLab2018.Nodes.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraLab2018.Nodes
{
    class IType : IDeclaration
    {
        public readonly string Name;
        public readonly IReadOnlyList<IType> TypeParameters;
        public string TypedName => Name + (TypeParameters.Count == 0 ? "" : ("[" + string.Join("", TypeParameters.Select(t => t.TypedName)) + "]"));
        public IType(string name, IReadOnlyList<IType> typeParameters)
        {
            Name = name;
            TypeParameters = typeParameters;
        }

        public override string ToString()
        {
            return FormattedString;
        }

        public bool Resolveable(Dictionary<String, IType> types) => TypeParameters.Aggregate(types.ContainsKey(Name), (a, p) => a || p.Resolveable(types));


        public IType Resolve(Dictionary<String, IType> types)
        {
            if (types.ContainsKey(Name))
            {
                var newT = types[Name];
                var newTypePars = TypeParameters.Select(p => p.Resolve(types)).ToList().AsReadOnly();
                return new IType(newT.Name, newT.TypeParameters);
            }
            var newTypeP = TypeParameters.Select(p => p.Resolve(types)).ToList().AsReadOnly();
            return new IType(Name, newTypeP);
        }

        public bool Constructable(IType t)
        {
            //if (t.TypeParameters.Count() == 0 && TypeParameters.Count == 0) return true;
            if (t.TypeParameters.Count() != TypeParameters.Count) return false;
            // return t.TypeParameters.Zip(TypeParameters, (a, b) => Tuple.Create(a, b)).Aggregate(true, (a, t1) => a && t1.Item1.Constructable(t1.Item2));
            return true;
        }

        public Tuple<bool,List<IType>> Constructable(AllTypes t)
        {
            if (t.typeConstructors.ContainsKey(Name))
            {
                var res = new List<IType>();
                return Tuple.Create(TypeParameters.Select(t1 => {
                    if (t1.TypeParameters.Count == 0)
                        return t.TryGetMyType(t1.TypedName) != null;
                    else
                    {
                        res.Add(t1);
                        var tres =  t1.Constructable(t);
                        res.AddRange(tres.Item2);
                        return tres.Item1;
                    }
                    }).Aggregate(true,(a,b) => a && b),res);
            }
            return Tuple.Create(false,new List<IType>());
        }


        public string FormattedString => $"{Name}" + (TypeParameters.Count == 0 ? "" : ("[" + string.Join(",", TypeParameters.Select(t => t.FormattedString)) + "]"));
    }
}
