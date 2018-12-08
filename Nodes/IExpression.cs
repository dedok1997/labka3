namespace ExtraLab2018.Nodes {
	interface IExpression : INode {
		void Accept(IExpressionVisitor visitor);
		T Accept<T>(IExpressionVisitor<T> visitor);
	}
}
