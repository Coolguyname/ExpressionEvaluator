using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionEvaluator.Core.Evaluating;

public sealed class EvaluatorException : Exception
{
    public int Position { get;  }

    public EvaluatorException(string message, int position)
        : base(message) 
    {
        Position = position;
    }
}
