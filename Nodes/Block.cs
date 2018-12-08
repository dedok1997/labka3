using System.Collections.Generic;
using System.Linq;
namespace ExtraLab2018.Nodes {
	sealed class Block : INode {
		public readonly IReadOnlyList<IStatement> Statements;
		public Block(IReadOnlyList<IStatement> statements) {
			Statements = statements;
		}
		public string FormattedString => "{\n" + string.Join("", Statements.Select(x => x.FormattedString)) + "}\n";
	}
}
