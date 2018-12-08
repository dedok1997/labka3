using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace ExtraLab2018.Lexer {
	sealed class Tokenizer {
		public static IEnumerable<Token> GetTokens(string text) {
			var regex = Regexes.Instance.CombinedRegex;
			var groupNames = Regexes.Instance.TokenGroupNames;
			int lastPos = 0;
			var matches = regex.Matches(text);
			Match match;
			Func<string, Exception> Error = (message) => {
				return new Exception(LexerUtils.MakeErrorMessage(text, lastPos, message));
			};
			foreach (Match m in matches) {
				match = m;
				if (lastPos < m.Index) {
					throw Error($"Пропустили '{text.Substring(lastPos, m.Index - lastPos)}'");
				}
				bool found = false;
				foreach (var kv in groupNames) {
					if (m.Groups[kv.Item2].Success) {
						if (found) {
							throw new Exception("Кривая регулярка нашла несколько вхождений");
						}
						found = true;
						yield return new Token(kv.Item1, m.Value, m.Index);
					}
				}
				if (!found) {
					throw new Exception("Кривая регулярка");
				}
				lastPos = m.Index + m.Length;
			}
			if (lastPos < text.Length) {
				throw Error($"Пропустили '{text.Substring(lastPos)}'");
			}
			yield return new Token(TokenType.EOF, "", text.Length);
		}
	}
}
