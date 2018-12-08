using System.Collections.Generic;
using System.Linq;
namespace ExtraLab2018.Nodes.Declarations {
	 class ClassDeclaration : IDeclaration {
		public readonly string Name;
		public readonly IReadOnlyList<IClassMember> Members;
		public ClassDeclaration(string name, IReadOnlyList<IClassMember> members) {
			Name = name;
			Members = members;
		}
		public string FormattedString => $"class {Name} {{\n{string.Join("", Members.Select(x => x.FormattedString)) }}}\n";
	}
}
