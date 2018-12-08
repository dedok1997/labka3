using System;
using System.Collections.Generic;
using System.Linq;
namespace ExtraLab2018.Nodes {
	sealed class ProgramNode : INode {
		public readonly IReadOnlyList<IDeclaration> Declarations;
		public readonly IReadOnlyList<IStatement> Statements;
        public readonly IReadOnlyList<IType> TypeCons;

        public ProgramNode(IReadOnlyList<IDeclaration> declarations, IReadOnlyList<IStatement> statements, IReadOnlyList<IType> typeCons)
        {
            Declarations = declarations ?? throw new ArgumentNullException(nameof(declarations));
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
            this.TypeCons = typeCons ?? throw new ArgumentNullException(nameof(typeCons));
        }

        public string FormattedString => string.Join("", Declarations.Select(x => x.FormattedString)) + "\n" + string.Join("", Statements.Select(x => x.FormattedString));
	}
}
