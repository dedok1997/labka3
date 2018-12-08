namespace ExtraLab2018.Nodes.Expressions {
	sealed class MemberAccess : IExpression {
		public readonly IExpression Obj;
		public readonly string Member;
		public MemberAccess(IExpression obj, string member) {
			Obj = obj;
			Member = member;
		}
		public string FormattedString => $"{Obj.FormattedString}.{Member}";
		public void Accept(IExpressionVisitor visitor) => visitor.VisitMemberAccess(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitMemberAccess(this);
	}
}
