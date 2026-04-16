using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionEvaluator.Core.Lexing;

public sealed class LexerException : Exception
{
    public LexerException(string message) : base(message) {}
}

