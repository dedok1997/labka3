namespace ExtraLab2018.Nodes {
	interface IStatement : INode {
		void Accept(IStatementVisitor visitor);
		T Accept<T>(IStatementVisitor<T> visitor);
	}
}
