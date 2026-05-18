using System;
using System.Linq;
using System.Collections.Generic;
using ExpressionEvaluator.Core.Parsing.Ast;
using ExpressionEvaluator.Core.Evaluating;

namespace ExpressionEvaluator.ViewModels;

public sealed class AstNodeViewModel
{
    public string Title { get; }
    public IReadOnlyList<AstNodeViewModel> Children { get; }

    private AstNodeViewModel(string label, IReadOnlyList<AstNodeViewModel> children)
    {
        Title = label;
        Children = children;
    }

    public static AstNodeViewModel FromExpression(Expression expr)
    {
        return expr switch
        {
            NumberLiteral n => new AstNodeViewModel($"Number: {n.Value}", Array.Empty<AstNodeViewModel>()),
            BooleanLiteral b => new AstNodeViewModel($"Boolean: {b.Value}", Array.Empty<AstNodeViewModel>()),
            Variable v => new AstNodeViewModel($"Variable: {v.Name}", Array.Empty<AstNodeViewModel>()),
            UnaryOp uop => new AstNodeViewModel($"UnaryOp: {uop.Operator}", [FromExpression(uop.Operand)]),
            BinaryOp bop => new AstNodeViewModel($"BinaryOp: {Evaluator.OperatorSymbol(bop.Operator)}", [FromExpression(bop.Left), FromExpression(bop.Right)]),
            FunctionCall fc => new AstNodeViewModel($"Function: {fc.Name}", [.. fc.Arguments.Select(FromExpression)]),
            _ => throw new ArgumentException($"Unknown type of expression: {expr.GetType().Name}")
        };
    }
}
