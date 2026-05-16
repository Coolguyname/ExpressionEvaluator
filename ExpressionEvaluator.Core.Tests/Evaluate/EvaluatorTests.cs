using ExpressionEvaluator.Core.Evaluating;
using ExpressionEvaluator.Core.Lexing;
using ExpressionEvaluator.Core.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionEvaluator.Core.Tests.Evaluate;

public class EvaluatorTests
{
    [Fact]
    public void Sqrt_works()
    {
        var ast = new Parser(new Lexer("sqrt(16)").Tokenize()).Parse();
        var result = new Evaluator().Evaluate(ast, Variables.Empty);
        Assert.Equal(new NumberValue(4.0), result);
    }

    [Fact]
    public void Min_picks_smaller()
    {
        var ast = new Parser(new Lexer("min(3, 5)").Tokenize()).Parse();
        var result = new Evaluator().Evaluate(ast, Variables.Empty);
        Assert.Equal(new NumberValue(3.0), result);
    }

    [Fact]
    public void Sqrt_of_negative_throws()
    {
        var ast = new Parser(new Lexer("sqrt(-4)").Tokenize()).Parse();
        var ex = Assert.Throws<EvaluatorException>(
            () => new Evaluator().Evaluate(ast, Variables.Empty));
        Assert.Contains("negative", ex.Message);
    }

    [Fact]
    public void Unknown_function_throws()
    {
        var ast = new Parser(new Lexer("foo(1)").Tokenize()).Parse();
        var ex = Assert.Throws<EvaluatorException>(
            () => new Evaluator().Evaluate(ast, Variables.Empty));
        Assert.Contains("Unknown function", ex.Message);
    }

    [Fact]
    public void Wrong_arity_throws()
    {
        var ast = new Parser(new Lexer("sqrt(1, 2)").Tokenize()).Parse();
        var ex = Assert.Throws<EvaluatorException>(
            () => new Evaluator().Evaluate(ast, Variables.Empty));
        Assert.Contains("expects 1 argument", ex.Message);
    }

    [Fact]
    public void Composition_of_functions()
    {
        var ast = new Parser(new Lexer("max(sqrt(16), abs(-5))").Tokenize()).Parse();
        var result = new Evaluator().Evaluate(ast, Variables.Empty);
        Assert.Equal(new NumberValue(5.0), result);
    }
}
