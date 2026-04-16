using System;
using System.Collections.Generic;
using ExpressionEvaluator.Core.Lexing;
using Xunit;
using System.Text;

namespace ExpressionEvaluator.Core.Tests.Lexing;

public class LexerTests
{
    [Fact]
    public void Tokenize_EmptyInput_ReturnsOnlyEof()
    {
        var lexer = new Lexer("");

        var tokens = lexer.Tokenize();

        Assert.Single(tokens);
        Assert.Equal(TokenKind.Eof, tokens[0].Kind);
        Assert.Equal(0, tokens[0].Position);
    }

    [Fact]
    public void Tokenize_OnlyWhitespace_ReturnsOnlyEof()
    {
        var lexer = new Lexer("  \t\r\n ");

        var tokens = lexer.Tokenize();

        Assert.Single(tokens);
        Assert.Equal(TokenKind.Eof, tokens[0].Kind);
        Assert.Equal(6, tokens[0].Position);
    }

    [Fact]
    public void Tokenize_UnknownCharacter_ThrowsLexerException()
    {
        var lexer = new Lexer("@");

        var ex = Assert.Throws<LexerException>(() => lexer.Tokenize());
        Assert.Contains("@", ex.Message);
    }
}
