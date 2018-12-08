using System.Collections.Generic;
using System.Linq;
namespace ExtraLab2018.Nodes.Expressions {
	sealed class CallExpression : IExpression {
		public readonly IExpression Function;
		public readonly IReadOnlyList<IExpression> Arguments;
		public string FormattedString => $"{Function.FormattedString}({string.Join(", ", Arguments.Select(x => x.FormattedString))})";
		public CallExpression(IExpression function, IReadOnlyList<IExpression> arguments) {
			Function = function;
			Arguments = arguments;
		}
		public void Accept(IExpressionVisitor visitor) => visitor.VisitCall(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitCall(this);
	}
}
