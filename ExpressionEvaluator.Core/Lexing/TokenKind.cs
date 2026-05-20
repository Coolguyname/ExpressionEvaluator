namespace ExpressionEvaluator.Core.Lexing;

public enum TokenKind
{
    Number, Identifier, True, False,

    Plus, Minus, Star, Slash, Percent, Caret, Lt, Gt,

    EqEq, NotEq, LtEq, GtEq,
    AndAnd, OrOr,
    Bang,

    And, Or, Not,

    LParen, RParen, Comma,

    Eof,
}
