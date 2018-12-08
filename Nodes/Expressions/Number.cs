namespace ExtraLab2018.Nodes.Expressions {
	sealed class Number : IExpression {
		public readonly string Lexeme;
		public Number(string lexeme) {
			Lexeme = lexeme;
		}
		public string FormattedString => Lexeme;
		public void Accept(IExpressionVisitor visitor) => visitor.VisitNumber(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitNumber(this);
	}
}
