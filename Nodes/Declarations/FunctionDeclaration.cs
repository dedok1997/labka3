using System.Collections.Generic;
using System.Linq;
namespace ExtraLab2018.Nodes.Declarations {
	sealed class FunctionDeclaration : IDeclaration {
		public readonly IType ReturnType;
		public readonly string Name;
		public readonly IReadOnlyList<ParameterNode> Parameters;
		public readonly Block Body;
		public static readonly string Keyword = "func";
		public FunctionDeclaration(IType returnType, string name, IReadOnlyList<ParameterNode> parameters, Block body) {
			ReturnType = returnType;
			Name = name;
			Parameters = parameters;
			Body = body;
		}
		public string FormattedString => $"{Keyword} {ReturnType.FormattedString} {Name}({string.Join(", ", Parameters.Select(x => x.FormattedString))}) {Body.FormattedString}";
	}
}
