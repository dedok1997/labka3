namespace ExtraLab2018.Nodes.Expressions {
	sealed class Paren : IExpression {
		public readonly IExpression Expr;
		public Paren(IExpression expr) {
			Expr = expr;
		}
		public string FormattedString => $"({Expr.FormattedString})";
		public void Accept(IExpressionVisitor visitor) => visitor.VisitParen(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitParen(this);
	}
}
