namespace ExtraLab2018.Nodes.Expressions {
	 class Identifier : IExpression {
		public readonly string Name;
		public Identifier(string name) {
			Name = name;
		}
		public string FormattedString => Name;
		public void Accept(IExpressionVisitor visitor) => visitor.VisitIdentifier(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitIdentifier(this);
	}
}
