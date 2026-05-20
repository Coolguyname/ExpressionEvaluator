using ExpressionEvaluator.Core.Lexing;
using ExpressionEvaluator.Core.Parsing.Ast;
using System.Globalization;

namespace ExpressionEvaluator.Core.Parsing;

public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _pos;

    public Parser(IReadOnlyList<Token> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        _tokens = tokens;
    }

    private Token Peek() => _tokens[_pos];

    private Token PeekNext() => _pos + 1 < _tokens.Count ? _tokens[_pos + 1] : _tokens[^1];

    private Token Advance()
    {
        var token = _tokens[_pos];
        if (token.Kind != TokenKind.Eof) _pos++;
        return token;
    }

    private bool Check(TokenKind kind) => Peek().Kind == kind;

    private bool Match(TokenKind kind)
    {
        if (!Check(kind)) return false;
        Advance();
        return true;
    }

    private Token Expect(TokenKind kind, string what)
    {
        if (Check(kind)) return Advance();
        throw new ParserException($"Expected {what} but found '{Peek().Lexeme}' at position {Peek().Position}.", Peek().Position);
    }

    public Expression Parse()
    {
        var expression = ParseExpression();

        Expect(TokenKind.Eof, "end of expression");
        return expression;
    }

    private Expression ParseExpression() => ParseOr();

    private Expression ParseOr()
    {
        var left = ParseAnd();

        while (Check(TokenKind.Or) || Check(TokenKind.OrOr))
        {
            var opToken = Advance();
            var right = ParseAnd();
            left = new BinaryOp(BinaryOperator.Or, left, right, opToken.Position);
        }

        return left;
    }

    private Expression ParseAnd()
    {
        var left = ParseNot();

        while (Check(TokenKind.And) || Check(TokenKind.AndAnd))
        {
            var opToken = Advance();
            var right = ParseNot();
            left = new BinaryOp(BinaryOperator.And, left, right, opToken.Position);
        }

        return left;
    }

    private Expression ParseNot()
    {
        if (Check(TokenKind.Not) || Check(TokenKind.Bang))
        {
            var notToken = Advance();
            var operand = ParseComparison();
            return new UnaryOp(UnaryOperator.Not, operand, notToken.Position);
        }
        return ParseComparison();
    }

    private Expression ParseComparison()
    {
        var left = ParseAdditive();

        if (Check(TokenKind.EqEq) || Check(TokenKind.NotEq) ||
            Check(TokenKind.Lt) || Check(TokenKind.LtEq) ||
            Check(TokenKind.Gt) || Check(TokenKind.GtEq))
        {
            var opToken = Advance();
            var right = ParseAdditive();
            var op = TokenToBinaryOperator(opToken.Kind);
            return new BinaryOp(op, left, right, opToken.Position);
        }

        return left;
    }

    private Expression ParseAdditive()
    {
        var left = ParseMultiplicative();

        while (Check(TokenKind.Plus) || Check(TokenKind.Minus))
        {
            var opToken = Advance();
            var right = ParseMultiplicative();
            var op = TokenToBinaryOperator(opToken.Kind);
            left = new BinaryOp(op, left, right, opToken.Position);
        }

        return left;
    }

    private Expression ParseMultiplicative()
    {
        var left = ParsePower();

        while (Check(TokenKind.Star) || Check(TokenKind.Slash) || Check(TokenKind.Percent))
        {
            var opToken = Advance();
            var right = ParsePower();
            var op = TokenToBinaryOperator(opToken.Kind);
            left = new BinaryOp(op, left, right, opToken.Position);
        }

        return left;
    }

    private Expression ParsePower()
    {
        var left = ParseUnary();

        if (Check(TokenKind.Caret))
        {
            var caretToken = Advance();
            var right = ParsePower();
            return new BinaryOp(BinaryOperator.Power, left, right, caretToken.Position);
        }

        return left;
    }

    private Expression ParseUnary()
    {
        if (Check(TokenKind.Minus))
        {
            var minusToken = Advance();
            var operand = ParsePrimary();
            return new UnaryOp(UnaryOperator.Negate, operand, minusToken.Position);
        }
        return ParsePrimary();
    }

    private Expression ParsePrimary()
    {
        var token = Peek();

        switch (token.Kind)
        {
            case TokenKind.Number:
                {
                    Advance();
                    var value = double.Parse(token.Lexeme, CultureInfo.InvariantCulture);
                    return new NumberLiteral(value, token.Position);
                }
            case TokenKind.True:
                {
                    Advance();
                    return new BooleanLiteral(true, token.Position);
                }
            case TokenKind.False:
                {
                    Advance();
                    return new BooleanLiteral(false, token.Position);
                }
            case TokenKind.Identifier:
                {
                    if (PeekNext().Kind == TokenKind.LParen)
                        return ParseFunctionCall();

                    Advance();
                    return new Variable(token.Lexeme, token.Position);
                }
            case TokenKind.LParen:
                {
                    Advance();
                    var inner = ParseExpression();
                    Expect(TokenKind.RParen, "')'");
                    return inner;
                }
            default:
                throw new ParserException($"Expected expression but found '{token.Lexeme}' at position {token.Position}.", token.Position);
        }
    }

    private FunctionCall ParseFunctionCall()
    {
        var nameToken = Advance();
        Expect(TokenKind.LParen, "'(' after function name");
        var arguments = new List<Expression>();

        if (!Check(TokenKind.RParen))
        {
            arguments.Add(ParseExpression());
            while (Match(TokenKind.Comma))
            {
                arguments.Add(ParseExpression());
            }
        }

        Expect(TokenKind.RParen, "')'");
        return new FunctionCall(nameToken.Lexeme, arguments, nameToken.Position);
    }

    private static BinaryOperator TokenToBinaryOperator(TokenKind kind) => kind switch
    {
        TokenKind.Plus => BinaryOperator.Add,
        TokenKind.Minus => BinaryOperator.Subtract,
        TokenKind.Star => BinaryOperator.Multiply,
        TokenKind.Slash => BinaryOperator.Divide,
        TokenKind.Percent => BinaryOperator.Modulo,
        TokenKind.Caret => BinaryOperator.Power,
        TokenKind.EqEq => BinaryOperator.Equal,
        TokenKind.NotEq => BinaryOperator.NotEqual,
        TokenKind.Lt => BinaryOperator.Less,
        TokenKind.LtEq => BinaryOperator.LessEqual,
        TokenKind.Gt => BinaryOperator.Greater,
        TokenKind.GtEq => BinaryOperator.GreaterEqual,
        TokenKind.And or TokenKind.AndAnd => BinaryOperator.And,
        TokenKind.Or or TokenKind.OrOr => BinaryOperator.Or,
        _ => throw new InvalidOperationException($"Token {kind} is not a binary operator.")
    };

}
