using System.Collections.Generic;
namespace ExtraLab2018.Nodes.Expressions {
	sealed class UnaryOperation : IExpression {
		public readonly UnaryOpType Operation;
		public readonly IExpression Expr;
		static readonly IReadOnlyDictionary<UnaryOpType, string> operators = new Dictionary<UnaryOpType, string>{
			{ UnaryOpType.UnaryMinus, "-" },
			{ UnaryOpType.LogicalNegation, "!" },
};
		public string FormattedString => $"{operators[Operation]} {Expr.FormattedString}";
		public UnaryOperation(UnaryOpType operation, IExpression expr) {
			Operation = operation;
			Expr = expr;
		}
		public void Accept(IExpressionVisitor visitor) => visitor.VisitUnary(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitUnary(this);
	}
}
