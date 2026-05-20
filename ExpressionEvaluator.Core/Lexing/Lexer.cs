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

        while (!IsAtEnd())
        {
            SkipWhiteSpace();
            if (IsAtEnd()) break;

            char c = Peek();
            if (char.IsDigit(c)) { tokens.Add(ReadNumber()); continue; }
            if (char.IsLetter(c) || c == '_') { tokens.Add(ReadIdentifier()); continue; }

            switch (c)
            {
                case '+': tokens.Add(SingleCharToken(TokenKind.Plus)); continue;
                case '-': tokens.Add(SingleCharToken(TokenKind.Minus)); continue;
                case '*': tokens.Add(SingleCharToken(TokenKind.Star)); continue;
                case '/': tokens.Add(SingleCharToken(TokenKind.Slash)); continue;
                case '%': tokens.Add(SingleCharToken(TokenKind.Percent)); continue;
                case '^': tokens.Add(SingleCharToken(TokenKind.Caret)); continue;
                case '(': tokens.Add(SingleCharToken(TokenKind.LParen)); continue;
                case ')': tokens.Add(SingleCharToken(TokenKind.RParen)); continue;
                case ',': tokens.Add(SingleCharToken(TokenKind.Comma)); continue;
                case '<':
                    if (PeekNext() == '=') { tokens.Add(TwoCharToken(TokenKind.LtEq)); }
                    else { tokens.Add(SingleCharToken(TokenKind.Lt)); }
                    continue;
                case '>':
                    if (PeekNext() == '=') { tokens.Add(TwoCharToken(TokenKind.GtEq)); }
                    else { tokens.Add(SingleCharToken(TokenKind.Gt)); }
                    continue;
                case '!':
                    if (PeekNext() == '=') { tokens.Add(TwoCharToken(TokenKind.NotEq)); }
                    else { tokens.Add(SingleCharToken(TokenKind.Bang)); }
                    continue;
                case '=':
                    if (PeekNext() == '=') { tokens.Add(TwoCharToken(TokenKind.EqEq)); continue; }
                    throw new LexerException($"Unexpected character '='. Did you mean '=='? Position: {_pos}.");
                case '&':
                    if (PeekNext() == '&') { tokens.Add(TwoCharToken(TokenKind.AndAnd)); continue; }
                    throw new LexerException($"Unexpected character '&'. Did you mean '&&'? Position: {_pos}.");
                case '|':
                    if (PeekNext() == '|') { tokens.Add(TwoCharToken(TokenKind.OrOr)); continue; }
                    throw new LexerException($"Unexpected character '|'. Did you mean '||'? Position: {_pos}.");
                default:
                    throw new LexerException($"Unexpected character '{c}' at position {_pos}.");
            }

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

    private TokenKind GetKeyWordKind(string lexeme) => lexeme switch
    {
        "true" => TokenKind.True,
        "false" => TokenKind.False,
        "and" or "AND" => TokenKind.And,
        "or" or "OR" => TokenKind.Or,
        "not" or "NOT" => TokenKind.Not,
        _ => TokenKind.Identifier,
    };

    private Token SingleCharToken(TokenKind kind)
    {
        int start = _pos;
        Advance();
        return new Token(kind, _source[start.._pos], start);
    }

    private Token TwoCharToken(TokenKind kind)
    {
        int start = _pos;
        Advance();
        Advance();
        return new Token(kind, _source[start.._pos], start);
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
