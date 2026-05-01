using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionEvaluator.Core.Parsing.Ast;

public sealed record FunctionCall(string Name, IReadOnlyList<Expression> Arguments, int Position) : Expression(Position);
