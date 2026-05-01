using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using ExpressionEvaluator.Core.Lexing;
using ExpressionEvaluator.Core.Parsing.Ast;
using System.ComponentModel.DataAnnotations;

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

    private bool Match(params TokenKind[] kinds)
    {
        foreach (var kind in kinds)
        {
            if (Check(kind))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Expect(TokenKind kind, string what)
    {
        if (Check(kind)) return Advance();
        throw new ParserException($"Expected {what} but found '{Peek().Lexeme}' at position {Peek().Position}.", Peek().Position);
    }

    private bool IsAtEnd() => Peek().Kind == TokenKind.Eof;

    private Expression ParseExpression() => throw new NotImplementedException();

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
