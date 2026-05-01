using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionEvaluator.Core.Parsing.Ast;

public enum BinaryOperator
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Power,

    Equal,
    NotEqual,
    Less, 
    LessEqual,
    Greater,
    GreaterEqual,

    And,
    Or
}
