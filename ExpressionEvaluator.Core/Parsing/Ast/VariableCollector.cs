using System;
using System.Collections.Generic;

namespace ExpressionEvaluator.Core.Parsing.Ast;

public static class VariableCollector
{
    public static IReadOnlyList<string> Collect(Expression expression)
    {
        List<string> result = new List<string>();
        Steps(expression, result);
        return result;
    }

    private static void Steps(Expression expression, List<string> result)
    {
        switch (expression)
        {
            case NumberLiteral: break;
            case BooleanLiteral: break;
            case Variable v: 
                if(!result.Contains(v.Name)) result.Add(v.Name); 
                break;
            case UnaryOp op: Steps(op.Operand, result); break;
            case BinaryOp bi: Steps(bi.Left, result); Steps(bi.Right, result); break;
            case FunctionCall fc: 
                foreach (var arg in fc.Arguments) Steps(arg, result); 
                break;
            default: throw new ArgumentException($"Unknown expression type: {expression?.GetType().Name ?? "null"}");
        }
    }
}
