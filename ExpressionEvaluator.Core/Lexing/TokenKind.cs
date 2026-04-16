using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionEvaluator.Core.Lexing;
public enum TokenKind
{
    // Literals
    Number, Identifier, True, False,

    //Single Char operators
    Plus, Minus, Star, Slash, Percent, Caret, Lt, Gt,

    //Multi Char operators
    EqEq, NotEq, LtEq, GtEq,
    AndAnd, OrOr,
    Bang,

    //Word form Logic opertors
    And, Or, Not,

    //Punctuation
    LParen, RParen, Comma,

    //Sentinel
    Eof,
}
