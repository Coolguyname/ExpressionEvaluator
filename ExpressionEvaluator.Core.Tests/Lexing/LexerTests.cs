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

    [Fact]
    public void Tokenize_Integer_ReturnsSingleNumberToken()
    {
        var lexer = new Lexer("42");

        var tokens = lexer.Tokenize();

        Assert.Equal(TokenKind.Number, tokens[0].Kind);
        Assert.Equal(0, tokens[0].Position);
        Assert.Equal("42", tokens[0].Lexeme);
        Assert.Equal(TokenKind.Eof, tokens[1].Kind);
    }

    [Fact]
    public void Tokenize_Decimal_ReturnsSingleNumberToken()
    {
        var lexer = new Lexer("3.14");

        var tokens = lexer.Tokenize();

        Assert.Equal(TokenKind.Number, tokens[0].Kind);
        Assert.Equal(0, tokens[0].Position);
        Assert.Equal("3.14", tokens[0].Lexeme);
    }

    [Fact]
    public void Tokenize_NumberStartingWithZero_Works()
    {
        var lexer = new Lexer("0.5");

        var tokens = lexer.Tokenize();

        Assert.Equal(TokenKind.Number, tokens[0].Kind);
        Assert.Equal(0, tokens[0].Position);
        Assert.Equal("0.5", tokens[0].Lexeme);
    }

    [Fact]
    public void Tokenize_InvalidDecimal_ThrowsLexerException()
    {
        var lexer = new Lexer("3.");

        Assert.Throws<LexerException>(() => lexer.Tokenize());
    }

    [Fact]
    public void Tokenize_SimpleIdentifier_ReturnsIdentifierToken()
    {
        var lexer = new Lexer("x");

        var tokens = lexer.Tokenize();

        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);
        Assert.Equal("x", tokens[0].Lexeme);
    }

    [Fact]
    public void Tokenize_LongerIdentifier_Works()
    {
        var lexer = new Lexer("radius_1");

        var tokens = lexer.Tokenize();

        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);
        Assert.Equal("radius_1", tokens[0].Lexeme);
    }

    [Fact]
    public void Tokenize_KeywordTrue_ReturnsTrueToken()
    {
        var lexer = new Lexer("true");

        var tokens = lexer.Tokenize();

        Assert.Equal(TokenKind.True, tokens[0].Kind);
        Assert.Equal("true", tokens[0].Lexeme);
    }

    [Fact]
    public void Tokenize_KeywordAnd_BothForms()
    {
        var lexer = new Lexer("and AND");

        var tokens = lexer.Tokenize();

        Assert.Equal(TokenKind.And, tokens[0].Kind);
        Assert.Equal(TokenKind.And, tokens[1].Kind);
        Assert.Equal("and", tokens[0].Lexeme);
        Assert.Equal("AND", tokens[1].Lexeme);
    }

    [Fact]
    public void Tokenize_IdentifierStartingWithKeyword_StaysIdentifier()
    {
        var lexer = new Lexer("trueValue");

        var tokens = lexer.Tokenize();

        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);
        Assert.Equal("trueValue", tokens[0].Lexeme);
    }
}
