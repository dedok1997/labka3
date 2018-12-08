using ExtraLab2018.Nodes.Expressions;
namespace ExtraLab2018.Nodes {
	interface IExpressionVisitor {
		void VisitBinary(BinaryOperation expression);
		void VisitConditional(ConditionalExpression expression);
		void VisitUnary(UnaryOperation expression);
		void VisitCall(CallExpression expression);
		void VisitParen(Paren expression);
		void VisitNumber(Number expression);
		void VisitIdentifier(Identifier expression);
		void VisitMemberAccess(MemberAccess expression);
		void VisitTypedExpression(TypedExpression expression);
	}
	interface IExpressionVisitor<T> {
		T VisitBinary(BinaryOperation expression);
		T VisitConditional(ConditionalExpression expression);
		T VisitUnary(UnaryOperation expression);
		T VisitCall(CallExpression expression);
		T VisitParen(Paren expression);
		T VisitNumber(Number expression);
		T VisitIdentifier(Identifier expression);
		T VisitMemberAccess(MemberAccess expression);
		T VisitTypedExpression(TypedExpression expression);
	}
}
