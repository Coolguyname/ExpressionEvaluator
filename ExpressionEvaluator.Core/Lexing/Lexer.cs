using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace ExpressionEvaluator.Core.Lexing;

public sealed class Lexer
{
    private readonly string _source;
    private int _pos;

    public Lexer(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
    }

    public IReadOnlyList<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while(!IsAtEnd())
        {
            SkipWhiteSpace();
            if (IsAtEnd()) break;

            throw new LexerException($"Unexpected character '{Peek()}' at position {_pos}.");
        }

        tokens.Add(new Token(TokenKind.Eof, "", _pos));
        return tokens;
    }

    private bool IsAtEnd() => _pos >= _source.Length;

    private char Peek() => _source[_pos];

    private char Advance() => _source[_pos++];

    private void SkipWhiteSpace()
    {
        while (!IsAtEnd() && IsWhiteSpace(Peek())) Advance();
    }

    private static bool IsWhiteSpace(char c)
        => c == ' ' || c == '\t' || c == '\r' || c == '\n';
}
