using ExtraLab2018.Nodes.Statements;
namespace ExtraLab2018.Nodes {
	interface IStatementVisitor {
		void VisitIf(IfStatement statement);
		void VisitWhile(WhileStatement statement);
		void VisitExpressionStatement(ExpressionStatement statement);
		void VisitAssignment(Assignment statement);
		void VisitBreak(Break statement);
		void VisitReturn(Return statement);
	}
	interface IStatementVisitor<T> {
		T VisitIf(IfStatement statement);
		T VisitWhile(WhileStatement statement);
		T VisitExpressionStatement(ExpressionStatement statement);
		T VisitAssignment(Assignment statement);
		T VisitBreak(Break statement);
		T VisitReturn(Return statement);
	}
}
