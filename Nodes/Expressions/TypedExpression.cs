namespace ExtraLab2018.Nodes.Expressions {
	sealed class TypedExpression : IExpression {
		public readonly IExpression Expr;
		public string Type { get; }
		public TypedExpression(IExpression expr, string type) {
			Expr = expr;
			Type = type;
		}
		public string FormattedString => $"{Expr.FormattedString}::{Type}";
		public void Accept(IExpressionVisitor visitor) => visitor.VisitTypedExpression(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitTypedExpression(this);
	}
}
