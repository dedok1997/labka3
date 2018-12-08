namespace ExtraLab2018.Nodes.Statements {
	sealed class Return : IStatement {
		public readonly IExpression Expr;
		public Return(IExpression expr) {
			Expr = expr;
		}
		public string FormattedString => $"return{(Expr == null ? "" : " " + Expr.FormattedString)};\n";
		public void Accept(IStatementVisitor visitor) => visitor.VisitReturn(this);
		public T Accept<T>(IStatementVisitor<T> visitor) => visitor.VisitReturn(this);
	}
}
