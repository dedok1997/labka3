using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtraLab2018.Nodes.Expressions {
	sealed class TypedIdentifier : Identifier {
        public readonly IReadOnlyList<IType> TypeParameters;
        public readonly String NonTypedName;


        public TypedIdentifier(string name, IReadOnlyList<IType> typeParameters):
            base(name + (typeParameters.Count == 0 ? "" : ("[" + string.Join("", typeParameters.Select(t => t.TypedName)) + "]")))
        {
            TypeParameters = typeParameters ?? throw new ArgumentNullException(nameof(typeParameters));
            NonTypedName = name;
        }

        public TypedIdentifier Resolve(Dictionary<String, IType> types)
        {
            if (types.ContainsKey(Name))
            {
                var newT = types[Name];
                var newTypePars = TypeParameters.Select(p => p.Resolve(types)).ToList().AsReadOnly();
                return new TypedIdentifier(newT.Name, newT.TypeParameters);
            }
            var newTypeP = TypeParameters.Select(p => p.Resolve(types)).ToList().AsReadOnly();
            return new TypedIdentifier(NonTypedName, newTypeP);
        }

    }
}
