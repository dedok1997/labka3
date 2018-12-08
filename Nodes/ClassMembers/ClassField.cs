
using System.Collections.Generic;
using System.Linq;
namespace ExtraLab2018.Nodes.ClassMembers {
	 class ClassField : IClassMember {
		public readonly IType Type;
		public readonly string Name;
		public ClassField(IType type, string name) {
			Type = type;
			Name = name;
		}
		 public string FormattedString => $"{Type.FormattedString} {Name};\n";
	}

}
