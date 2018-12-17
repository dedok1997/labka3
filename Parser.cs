using ExtraLab2018.Lexer;
using ExtraLab2018.Nodes;
using ExtraLab2018.Nodes.ClassMembers;
using ExtraLab2018.Nodes.Declarations;
using ExtraLab2018.Nodes.Expressions;
using ExtraLab2018.Nodes.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ExtraLab2018 {
	sealed class Parser {
		readonly IReadOnlyList<Token> tokens;
		readonly string source;
        private List<IType> typeCons = new List<IType>(); 
		int position = 0;
		Token CurrentToken => tokens[position];
		Parser(IReadOnlyList<Token> tokens, string source) {
			this.tokens = tokens;
			this.source = source;
		}
#if DEBUG
		string[] DebugCurrentPosition => LexerUtils.FormatLines(source, CurrentToken.Position,
			inlinePointer: true,
			pointer: " <|> "
			).ToArray();
		string DebugCurrentLine => string.Join("", LexerUtils.FormatLines(source, CurrentToken.Position,
			linesAround: 0,
			inlinePointer: true,
			pointer: " <|> "
			).ToArray());
#endif
		#region stuff
		static bool IsNotWhitespace(Token t) {
			switch (t.Type) {
				case TokenType.Whitespaces:
				case TokenType.SingleLineComment:
				case TokenType.MultiLineComment:
					return false;
			}
			return true;
		}
		void ExpectEof() {
			if (!IsType(TokenType.EOF)) {
				throw ParserError($"Не допарсили до конца, остался {CurrentToken}");
			}
		}
		void ReadNextToken() {
			position += 1;
		}
		void Reset() {
			position = 0;
		}
		Exception ParserError(string message) {
			return new Exception(LexerUtils.MakeErrorMessage(source, CurrentToken.Position, message));
		}
		bool SkipIf(string s) {
			if (CurrentIs(s)) {
				ReadNextToken();
				return true;
			}
			return false;
		}
		bool CurrentIs(string s) => string.Equals(CurrentToken.Lexeme, s, StringComparison.Ordinal);
		bool IsType(TokenType type) => CurrentToken.Type == type;
		void Expect(string s) {
			if (!SkipIf(s)) {
				throw ParserError($"Ожидали \"{s}\", получили {CurrentToken}");
			}
		}
		#endregion
		public static ProgramNode Parse(string source) {
			var tokens = Tokenizer.GetTokens(source).Where(IsNotWhitespace).ToList();
			var parser = new Parser(tokens, source);
			return parser.ParseProgram();
		}
		ProgramNode ParseProgram() {
			Reset();
			var declarations = new List<IDeclaration>();
			while (true) {
				var declaration = TryParseDeclaration();
				if (declaration == null) {
					break;
				}
				declarations.Add(declaration);
			}
			var statements = new List<IStatement>();
			while (!IsType(TokenType.EOF)) {
				statements.Add(ParseStatement());
			}
			var result = new ProgramNode(declarations, statements,typeCons);
			ExpectEof();
			return result;
		}
		IDeclaration TryParseDeclaration() {
			if (CurrentIs("class")) {
				return ParseClassOrNeClass();
			}
            if (CurrentIs(FunctionDeclaration.Keyword)) {
				return ParseFunctionDeclaration();
			}
			return null;
		}

        ClassDeclaration ParseClassOrNeClass()
        {
            Expect("class");
            var name = ParseIdentifier();
            if (CurrentIs("[")) return ParseTypeConstructorDeclaration(name);
            else return ParseClassDeclaration(name);
        }

        ClassDeclaration ParseClassDeclaration(string name) {
			var members = ParseClassMembers();
			return new ClassDeclaration(name, members);
		}

        TypeConstructorDeclaration ParseTypeConstructorDeclaration(string name)
        {
            var typeParameters = parseTypeParameters();
            var members = ParseClassMembers();
            return new TypeConstructorDeclaration(name, members,typeParameters);
        }

        IReadOnlyList<IType> parseTypeParameters()
        {
            Expect("[");
            var parameters = new List<IType>();
            if (!CurrentIs("]"))
            {
                parameters.Add(ParseType());
                while (SkipIf(","))
                {
                    parameters.Add(ParseType());
                }
            }
            Expect("]");
            return parameters.AsReadOnly();
        }


        IReadOnlyList<IClassMember> ParseClassMembers() {
			Expect("{");
			var members = new List<IClassMember>();
			while (!SkipIf("}")) {
				members.Add(ParseClassMember());
			}
			return members;
		}

		IClassMember ParseClassMember() {
			var type = ParseType();
			var name = ParseIdentifier();
			if (SkipIf(";")) {
				return new ClassField(type, name);
			}
			var parameters = ParseParameters();
			var body = ParseBlock();
			return new ClassMethod(type, name, parameters, body);
		}
		IReadOnlyList<ParameterNode> ParseParameters() {
			Expect("(");
			var parameters = new List<ParameterNode>();
			if (!CurrentIs(")")) {
				parameters.Add(ParseParameter());
				while (SkipIf(",")) {
					parameters.Add(ParseParameter());
				}
			}
			Expect(")");
			return parameters;
		}
		ParameterNode ParseParameter() {
			var type = ParseType();
			var name = ParseIdentifier();
			return new ParameterNode(type, name);
		}
		FunctionDeclaration ParseFunctionDeclaration() {
			Expect(FunctionDeclaration.Keyword);
			var type = ParseType();
			var name = ParseIdentifier();
			var parameters = ParseParameters();
			var body = ParseBlock();
			return new FunctionDeclaration(type, name, parameters, body);
		}
		Block ParseBlock() {
			Expect("{");
			var statements = new List<IStatement>();
			while (!SkipIf("}")) {
				statements.Add(ParseStatement());
			}
			return new Block(statements);
		}
		IStatement ParseStatement() {
			if (SkipIf("if")) {
				Expect("(");
				var condition = ParseExpression();
				Expect(")");
				var block = ParseBlock();
				return new IfStatement(condition, block);
			}
			if (SkipIf("while")) {
				Expect("(");
				var condition = ParseExpression();
				Expect(")");
				var block = ParseBlock();
				return new WhileStatement(condition, block);
			}
			if (SkipIf("break")) {
				Expect(";");
				return new Break();
			}
			if (SkipIf("return")) {
				IExpression expr = null;
				if (!SkipIf(";")) {
					expr = ParseExpression();
					Expect(";");
				}
				return new Return(expr);
			}
			var expression = ParseExpression();
			if (SkipIf("=")) {
				var restAssigmentExpression = ParseExpression();
				Expect(";");
				return new Assignment(expression, null, restAssigmentExpression);
			}
			else if (SkipIf(":")) {
				var type = ParseType();
				Expect("=");
				var restAssigmentExpression = ParseExpression();
				Expect(";");
				return new Assignment(expression, type, restAssigmentExpression);
			}
			else {
				Expect(";");
				return new ExpressionStatement(expression);
			}
		}
		IType ParseType() {
            var name = ParseIdentifier();
            if (CurrentIs("["))
            {
                var pars = parseTypeParameters();
                typeCons.Add(new IType(name, pars));
                return typeCons.Last();
            }
            return new IType(name, new IType[0]);
		}

      

        string ParseIdentifier() {
			if (!IsType(TokenType.Identifier)) {
				throw ParserError($"Ожидали идентификатор, получили {CurrentToken}");
			}
			var lexeme = CurrentToken.Lexeme;
			ReadNextToken();
			return lexeme;
		}
		#region expressions
		IExpression ParseExpression() {
			return ParseConditionalExpression();
		}
		IExpression ParseExpressionType(IExpression expression) {
			if (SkipIf("::")) {
				return new TypedExpression(expression, ParseIdentifier());
			}
			return expression;
		}
		IExpression ParseConditionalExpression() {
			var condition = ParseLogicalOrExpression();
			if (SkipIf("?")) {
				var trueExpr = ParseExpression();
				Expect(":");
				var falseExpr = ParseExpression();
				return ParseExpressionType(new ConditionalExpression(condition, trueExpr, falseExpr));
			}
			return condition;
		}
		IExpression ParseLogicalOrExpression() {
			var left = ParseLogicalAndExpression();
			while (true) {
				if (SkipIf("||")) {
					var right = ParseLogicalAndExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.LogicalOr, right));
				}
				else break;
			}
			return left;
		}
		IExpression ParseLogicalAndExpression() {
			var left = ParseEqualityExpression();
			while (true) {
				if (SkipIf("&&")) {
					var right = ParseEqualityExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.LogicalAnd, right));
				}
				else break;
			}
			return left;
		}
		IExpression ParseEqualityExpression() {
			var left = ParseRelationalExpression();
			while (true) {
				if (SkipIf("==")) {
					var right = ParseRelationalExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.Equal, right));
				}
				else if (SkipIf("!=")) {
					var right = ParseRelationalExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.NotEqual, right));
				}
				else break;
			}
			return left;
		}
		IExpression ParseRelationalExpression() {
			var left = ParseAdditiveExpression();
			while (true) {
				if (SkipIf("<")) {
					var right = ParseAdditiveExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.Less, right));
				}
				else if (SkipIf("<=")) {
					var right = ParseAdditiveExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.LessEqual, right));
				}
				else if (SkipIf(">")) {
					var right = ParseAdditiveExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.Greater, right));
				}
				else if (SkipIf(">=")) {
					var right = ParseAdditiveExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.GreaterEqual, right));
				}
				else break;
			}
			return left;
		}
		IExpression ParseAdditiveExpression() {
			var left = ParseMultiplicativeExpression();
			while (true) {
				if (SkipIf("+")) {
					var right = ParseMultiplicativeExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.Addition, right));
				}
				else if (SkipIf("-")) {
					var right = ParseMultiplicativeExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.Subtraction, right));
				}
				else break;
			}
			return left;
		}
		IExpression ParseMultiplicativeExpression() {
			var left = ParseUnaryExpression();
			while (true) {
				if (SkipIf("*")) {
					var right = ParseUnaryExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.Multiplication, right));
				}
				else if (SkipIf("/")) {
					var right = ParseUnaryExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.Division, right));
				}
				else if (SkipIf("%")) {
					var right = ParseUnaryExpression();
					left = ParseExpressionType(new BinaryOperation(left, BinaryOpType.Remainder, right));
				}
				else break;
			}
			return left;
		}
		IExpression ParseUnaryExpression() {
			if (SkipIf("-")) {
				var expression = ParseUnaryExpression();
				return ParseExpressionType(new UnaryOperation(UnaryOpType.UnaryMinus, expression));
			}
			else if (SkipIf("!")) {
				var expression = ParseUnaryExpression();
				return ParseExpressionType(new UnaryOperation(UnaryOpType.LogicalNegation, expression));
			}
			return ParsePrimary();
		}
		IExpression ParsePrimary() {
			var expression = ParsePrimitive();
			while (true) {
				if (SkipIf("(")) {
					var arguments = new List<IExpression>();
					if (!CurrentIs(")")) {
						arguments.Add(ParseExpression());
                        
                        while (SkipIf(",")) {
							arguments.Add(ParseExpression());
						}
					}
                   
					Expect(")");
					expression = ParseExpressionType(new CallExpression(expression, arguments));
				}
				else if (SkipIf(".")) {
					var member = ParseIdentifier();
					expression = ParseExpressionType(new MemberAccess(expression, member));
				}
				else {
					break;
				}
			}
			return expression;
		}
		IExpression ParsePrimitive() {
			if (SkipIf("(")) {
				var expression = new Paren(ParseExpression());
				Expect(")");
				return ParseExpressionType(expression);
			}
			if (IsType(TokenType.NumberLiteral)) {
				var lexeme = CurrentToken.Lexeme;
				ReadNextToken();
				return ParseExpressionType(new Number(lexeme));
			}
			else if (IsType(TokenType.Identifier)) {
				var lexeme = CurrentToken.Lexeme;
				ReadNextToken();
                if (CurrentIs("["))
                {
                    var pars = parseTypeParameters();
                    typeCons.Add(new IType(lexeme, pars));
				    return ParseExpressionType(new TypedIdentifier(lexeme, pars));

                }
                return ParseExpressionType(new Identifier(lexeme));
			}
			throw ParserError($"Ожидали идентификатор, число или скобку, получили {CurrentToken}");
		}
		#endregion
	}
}
