using System;
using System.Collections.Generic;
using System.Linq;
namespace ExtraLab2018.Nodes.Declarations
{
    sealed class TypeConstructorDeclaration : ClassDeclaration
    {
        public readonly IReadOnlyList<IType> TypeParameters;
        public TypeConstructorDeclaration(string name, IReadOnlyList<IClassMember> members, IReadOnlyList<IType> typeParameters):base(name,members)
        {
            TypeParameters = typeParameters;
        }

        public bool Constructable(IReadOnlyList<IType> typeParameters)
        {
            if (typeParameters.Count() != TypeParameters.Count) return false;
            return true;
        }

        public string FormattedString => $"type {Name}[{ string.Join(",", TypeParameters) }] {{\n{string.Join("", Members.Select(x => x.FormattedString)) }}}\n";
    }
}


