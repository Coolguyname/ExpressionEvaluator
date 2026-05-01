using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionEvaluator.Core.Parsing.Ast;

public sealed record NumberLiteral(double Value, int Position) : Expression(Position);

public sealed record BooleanLiteral(bool Value, int Position) : Expression(Position);

public sealed record Variable(string Name, int Position) : Expression(Position);