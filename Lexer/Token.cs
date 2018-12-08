using System;
namespace ExtraLab2018.Lexer {
	sealed class Token {
		public readonly TokenType Type;
		public readonly string Lexeme;
		public readonly int Position;
		public Token(TokenType type, string lexeme, int position) {
			if (type != TokenType.EOF) {
				var regex = Regexes.Instance.GetTokenRegex(type);
				if (!regex.IsMatch(lexeme)) {
					throw new Exception($"Лексема {lexeme} не подходит под регулярку {regex}");
				}
			}
			Type = type;
			Lexeme = lexeme;
			Position = position;
		}
		public override string ToString() => $"{Type} \"{Lexeme}\"";
	}
}
