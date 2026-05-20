namespace ExpressionEvaluator.Core.Lexing;

public readonly record struct Token(TokenKind Kind, string Lexeme, int Position);

