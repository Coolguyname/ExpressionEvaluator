using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionEvaluator.Core.Parsing.Ast;

public sealed record UnaryOp(UnaryOperator Operator, Expression Operand, int Position) : Expression(Position);

public sealed record BinaryOp(BinaryOperator Operator, Expression Left, Expression Right, int Position) : Expression(Position);
