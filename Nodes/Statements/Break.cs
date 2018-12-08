namespace ExtraLab2018.Nodes.Statements {
	sealed class Break : IStatement {
		public string FormattedString => "break;\n";
		public void Accept(IStatementVisitor visitor) => visitor.VisitBreak(this);
		public T Accept<T>(IStatementVisitor<T> visitor) => visitor.VisitBreak(this);
	}
}
