using System;
using System.Collections.Generic;
using ExpressionEvaluator.Core.Parsing.Ast;
using System.Text;
using System.Numerics;
using System.Reflection.Emit;

namespace ExpressionEvaluator.Core.Evaluating;

public sealed class Evaluator
{
    public Value Evaluate(Expression expression, Variables variables)
    {
        return expression switch
        {
            NumberLiteral n => new NumberValue(n.Value),
            BooleanLiteral b => new BooleanValue(b.Value),
            Variable v => variables.Get(v.Name, v.Position),
            UnaryOp u => EvaluateUnary(u, variables),
            _ => throw new EvaluatorException($"Unsupported expression type: {expression.GetType().Name}", expression.Position)
        };    
    }

    private Value EvaluateUnary(UnaryOp node, Variables variables) 
    {
        var operand = Evaluate(node.Operand, variables);

        return node.Operator switch
        {
            UnaryOperator.Negate => operand switch
            {
                NumberValue n => new NumberValue(-n.Number),
                _ => throw new EvaluatorException($"Operator '-' requires Number, got {operand.TypeName}",
                node.Position)
            },
            UnaryOperator.Not => operand switch
            {
                BooleanValue b => new BooleanValue(!b.Boolean),
                _ => throw new EvaluatorException($"Operator 'NOT' requires Boolean, go {operand.TypeName}",
                node.Position)
            },
            _ => throw new EvaluatorException($"Unknown unary operator: {node.Operator}",
            node.Position)
        };
    }
}
