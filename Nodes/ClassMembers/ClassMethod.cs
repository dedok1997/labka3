using System.Collections.Generic;
using System.Linq;
namespace ExtraLab2018.Nodes.ClassMembers {
	sealed class ClassMethod : IClassMember {
		public readonly IType ReturnType;
		public readonly string Name;
		public readonly IReadOnlyList<ParameterNode> Parameters;
		public readonly Block Body;
		public ClassMethod(IType returnType, string name, IReadOnlyList<ParameterNode> parameters, Block body) {
			ReturnType = returnType;
			Name = name;
			Parameters = parameters;
			Body = body;
		}
		public string FormattedString => $"{ReturnType} {Name}({string.Join(", ", Parameters.Select(x => x.FormattedString))}) {Body.FormattedString}";
	}
}
