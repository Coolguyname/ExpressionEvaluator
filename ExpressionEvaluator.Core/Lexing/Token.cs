using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ExpressionEvaluator.Core.Lexing;

public readonly record struct Token(TokenKind Kind, string Lexeme, int Position);

