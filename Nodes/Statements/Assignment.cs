namespace ExtraLab2018.Nodes.Statements {
	sealed class Assignment : IStatement {
		public readonly IExpression Target;
		public readonly IType Type;
		public readonly IExpression Expr;
		public string FormattedString => $"{Target.FormattedString}{(Type != null ? " : " + Type.FormattedString : "")} = {Expr.FormattedString};\n";
		public Assignment(IExpression target, IType type, IExpression expr) {
			Target = target;
			Type = type;
			Expr = expr;
		}
		public void Accept(IStatementVisitor visitor) => visitor.VisitAssignment(this);
		public T Accept<T>(IStatementVisitor<T> visitor) => visitor.VisitAssignment(this);
	}
}
