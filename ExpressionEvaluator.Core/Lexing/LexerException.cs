using System;

namespace ExpressionEvaluator.Core.Lexing;

public sealed class LexerException : Exception
{
    public LexerException(string message) : base(message) {}
}

