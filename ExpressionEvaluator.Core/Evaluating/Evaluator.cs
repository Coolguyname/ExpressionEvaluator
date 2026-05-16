using System;
using System.Collections.Generic;
using ExpressionEvaluator.Core.Parsing.Ast;
using System.Text;
using System.Numerics;
using System.Reflection.Emit;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ExpressionEvaluator.Core.Evaluating;

internal delegate Value BuiltinFunction(IReadOnlyList<Value> arguments, int position);

public sealed class Evaluator
{
    private readonly IReadOnlyDictionary<string, BuiltinFunction> builtins;

    public Evaluator()
    {
        builtins = new Dictionary<string, BuiltinFunction>
        {
            ["sqrt"] = Sqrt,
            ["abs"] = Abs,
            ["min"] = Min,
            ["max"] = Max,
            ["pow"] = Pow
        };
    }

    public Value Evaluate(Expression expression, Variables variables)
    {
        return expression switch
        {
            NumberLiteral n => new NumberValue(n.Value),
            BooleanLiteral b => new BooleanValue(b.Value),
            Variable v => variables.Get(v.Name, v.Position),
            UnaryOp u => EvaluateUnary(u, variables),
            BinaryOp b => EvaluateBinary(b, variables),
            FunctionCall f => EvaluateFunctionCall(f, variables),
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

    private Value EvaluateBinary(BinaryOp node, Variables variables)
    {
        return node.Operator switch
        {
            BinaryOperator.Add or BinaryOperator.Subtract or BinaryOperator.Multiply or
            BinaryOperator.Divide or BinaryOperator.Modulo or BinaryOperator.Power => EvaluateArithmetic(node, variables),

            BinaryOperator.Equal or BinaryOperator.NotEqual or BinaryOperator.Less or
            BinaryOperator.LessEqual or BinaryOperator.Greater or BinaryOperator.GreaterEqual => EvaluateComparison(node, variables),

            BinaryOperator.And or BinaryOperator.Or => EvaluateLogical(node, variables),

            _ => throw new EvaluatorException($"Unknown binary operator: {node.Operator}",
            node.Position)
        };
    }

    private Value EvaluateArithmetic(BinaryOp node, Variables variables)
    {
        var left = Evaluate(node.Left, variables);
        var right = Evaluate(node.Right, variables);

        if (left is not NumberValue ln)
            throw new EvaluatorException($"Operator '{OperatorSymbol(node.Operator)}' requires Number on left, got {left.TypeName}",
                                         node.Position);
        if (right is not NumberValue rn)
            throw new EvaluatorException($"Operator '{OperatorSymbol(node.Operator)}' requires Number on right, got {right.TypeName}",
                                         node.Position);

        return node.Operator switch
        {
            BinaryOperator.Add => new NumberValue(ln.Number + rn.Number),
            BinaryOperator.Subtract => new NumberValue(ln.Number - rn.Number),
            BinaryOperator.Multiply => new NumberValue(ln.Number * rn.Number),
            BinaryOperator.Divide => rn.Number == 0
                ? throw new EvaluatorException("Division by zero", node.Position)
                : new NumberValue(ln.Number / rn.Number),
            BinaryOperator.Modulo => rn.Number == 0
            ? throw new EvaluatorException("Modulo by zero", node.Position)
            : new NumberValue(ln.Number % rn.Number),
            BinaryOperator.Power => new NumberValue(Math.Pow(ln.Number, rn.Number)),
            _ => throw new EvaluatorException($"Unexpected arithmetic operator: {node.Operator}",
                node.Position)
        };
    }

    private Value EvaluateComparison(BinaryOp node, Variables variables)
    {
        var left = Evaluate(node.Left, variables);
        var right = Evaluate(node.Right, variables);

        if (node.Operator is BinaryOperator.Equal or BinaryOperator.NotEqual)
        {
            if (left.GetType() != right.GetType())
                throw new EvaluatorException($"Cannot compare {left.TypeName} with {right.TypeName}",
                                             node.Position);
            var equal = left.Equals(right);
            return new BooleanValue(node.Operator == BinaryOperator.Equal ? equal : !equal);
        }

        if (left is not NumberValue ln)
            throw new EvaluatorException($"Operator '{OperatorSymbol(node.Operator)}' requires Number on left, got {left.TypeName}",
                                         node.Position);
        if (right is not NumberValue rn)
            throw new EvaluatorException($"Operator '{OperatorSymbol(node.Operator)}' requires Number on right, got {right.TypeName}",
                                         node.Position);

        return node.Operator switch
        {
            BinaryOperator.Less => new BooleanValue(ln.Number < rn.Number),
            BinaryOperator.LessEqual => new BooleanValue(ln.Number <= rn.Number),
            BinaryOperator.Greater => new BooleanValue(ln.Number > rn.Number),
            BinaryOperator.GreaterEqual => new BooleanValue(ln.Number >= rn.Number),
            _ => throw new EvaluatorException($"Unexpected comparison operator: {node.Operator}",
                                              node.Position)
        };
    }

    private Value EvaluateLogical(BinaryOp node, Variables variables)
    {
        var left = Evaluate(node.Left, variables);

        if (left is not BooleanValue lb)
            throw new EvaluatorException($"Operator '{OperatorSymbol(node.Operator)}' requires Boolean on left, got {left.TypeName}",
                                         node.Position);

        if (node.Operator == BinaryOperator.And && !lb.Boolean)
            return new BooleanValue(false);

        if (node.Operator == BinaryOperator.Or && lb.Boolean)
            return new BooleanValue(true);

        var right = Evaluate(node.Right, variables);

        if (right is not BooleanValue rb)
            throw new EvaluatorException($"Operator '{OperatorSymbol(node.Operator)}' requires Boolean on right, got {right.TypeName}",
                                         node.Position);

        return node.Operator switch
        {
            BinaryOperator.And => new BooleanValue(lb.Boolean && rb.Boolean),
            BinaryOperator.Or => new BooleanValue(lb.Boolean || rb.Boolean),
            _ => throw new EvaluatorException($"Unexpected logical operator: {node.Operator}", 
                                              node.Position)
        };
    }

    private static string OperatorSymbol(BinaryOperator op) => op switch
    {
        BinaryOperator.Add => "+",
        BinaryOperator.Subtract => "-",
        BinaryOperator.Multiply => "*",
        BinaryOperator.Divide => "/",
        BinaryOperator.Modulo => "%",
        BinaryOperator.Power => "^",
        BinaryOperator.Equal => "==",
        BinaryOperator.NotEqual => "!=",
        BinaryOperator.Less => "<",
        BinaryOperator.LessEqual => "<=",
        BinaryOperator.Greater => ">",
        BinaryOperator.GreaterEqual => ">=",
        BinaryOperator.And => "AND/&&",
        BinaryOperator.Or => "OR/||",
        _ => op.ToString()
    };

    private Value EvaluateFunctionCall(FunctionCall node, Variables variables)
    {
        if (!builtins.TryGetValue(node.Name, out var function))
            throw new EvaluatorException($"Unknown function '{node.Name}'",
                                         node.Position);

        var evaluatedArgs = new List<Value>(node.Arguments.Count);
        foreach (var arg in node.Arguments)
        {
            evaluatedArgs.Add(Evaluate(arg, variables));
        }

        return function(evaluatedArgs, node.Position);
    }

    private static Value Sqrt(IReadOnlyList<Value> args, int position)
    {
        ExpectArity("sqrt", args, 1, position);
        var x = ExpectNumber("sqrt", args[0], 0, position);
        if (x < 0)
            throw new EvaluatorException("sqrt of negative number", position);
        return new NumberValue(Math.Sqrt(x));
    }

    private static Value Abs(IReadOnlyList<Value> args, int position)
    {
        ExpectArity("abs", args, 1, position);
        var x = ExpectNumber("abs", args[0], 0, position);
        return new NumberValue(Math.Abs(x));
    }

    private static Value Min(IReadOnlyList<Value> args, int position)
    {
        ExpectArity("min", args, 2, position);
        var a = ExpectNumber("min", args[0], 0, position);
        var b = ExpectNumber("min", args[1], 1, position);
        return new NumberValue(Math.Min(a, b));
    }

    private static Value Max(IReadOnlyList<Value> args, int position)
    {
        ExpectArity("max", args, 2, position);
        var a = ExpectNumber("max", args[0], 0, position);
        var b = ExpectNumber("max", args[1], 1, position);
        return new NumberValue(Math.Max(a, b));
    }

    private static Value Pow(IReadOnlyList<Value> args, int position)
    {
        ExpectArity("pow", args, 2, position);
        var a = ExpectNumber("pow", args[0], 0, position);
        var b = ExpectNumber("pow", args[1], 1, position);
        return new NumberValue(Math.Pow(a, b));
    }

    private static void ExpectArity(string functionName, IReadOnlyList<Value> args, int expected, int position)
    {
        if (args.Count != expected)
            throw new EvaluatorException($"Function '{functionName}' expects {expected} argument(s), got {args.Count}",
                                         position);
    }

    private static double ExpectNumber(string functionName, Value value, int argIndex, int position)
    {
        if (value is NumberValue n)
            return n.Number;

        throw new EvaluatorException($"Function '{functionName}' expects Number at argument {argIndex + 1}, got {value.TypeName}",
                                     position);
    }
}
