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

            char c = Peek();
            if (char.IsDigit(c))
            {
                tokens.Add(ReadNumber());
                continue;
            } 
            else if (char.IsLetter(c) || c == '_')
            {
                tokens.Add(ReadIdentifier());
                continue;
            }

            throw new LexerException($"Unexpected character '{Peek()}' at position {_pos}.");
        }

        tokens.Add(new Token(TokenKind.Eof, "", _pos));
        return tokens;
    }

    private Token ReadNumber()
    {
        int start = _pos;

        while (!IsAtEnd() && char.IsDigit(Peek()))
        {
            Advance();
        }

        if (!IsAtEnd() && Peek() == '.') 
        {
            if (char.IsDigit(PeekNext()))
            {
                Advance();

                while (!IsAtEnd() && char.IsDigit(Peek()))
                {
                    Advance();
                }
            } 
            else
            {
                throw new LexerException($"Invalid number: missing digits after decimal point at position {_pos}.");
            }
        }

        string lexeme = _source[start.._pos];
        return new Token(TokenKind.Number, lexeme, start);
    }

    private Token ReadIdentifier()
    {
        int start = _pos;

        while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            Advance();
        }

        string lexeme = _source[start.._pos];

        return new Token(GetKeyWordKind(lexeme), lexeme, start);
    }

    private TokenKind GetKeyWordKind(string lexeme)
    {
        switch (lexeme.ToLower()) 
        {
            case "true": return TokenKind.True;
            case "false": return TokenKind.False;
            case "and": return TokenKind.And;
            case "or": return TokenKind.Or;
            case "not": return TokenKind.Not;
        }
        return TokenKind.Identifier;
    }

    private bool IsAtEnd() => _pos >= _source.Length;

    private char Peek() => _source[_pos];

    private char PeekNext() => (_pos + 1) < _source.Length ? _source[_pos + 1] : '\0';

    private char Advance() => _source[_pos++];

    private void SkipWhiteSpace()
    {
        while (!IsAtEnd() && IsWhiteSpace(Peek())) Advance();
    }

    private static bool IsWhiteSpace(char c)
        => c == ' ' || c == '\t' || c == '\r' || c == '\n';
}
