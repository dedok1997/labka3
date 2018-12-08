namespace ExtraLab2018.Nodes {
	sealed class ParameterNode : INode {
		public readonly IType Type;
		public readonly string Name;
		public ParameterNode(IType type, string name) {
			Type = type;
			Name = name;
		}
		public string FormattedString => $"{Type.FormattedString} {Name}";
	}
}
