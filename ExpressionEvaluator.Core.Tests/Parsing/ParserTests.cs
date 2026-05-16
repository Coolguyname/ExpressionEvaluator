using ExpressionEvaluator.Core.Evaluating;
using ExpressionEvaluator.Core.Lexing;
using ExpressionEvaluator.Core.Parsing;
using ExpressionEvaluator.Core.Parsing.Ast;
using Xunit;

namespace ExpressionEvaluator.Core.Tests.Parsing;

public class ParserTests
{
    private static Expression Parse(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        return parser.Parse();
    }

    [Fact]
    public void Parse_Number_ReturnsNumberLiteral()
    {
        var ast = Parse("42");

        var number = Assert.IsType<NumberLiteral>(ast);
        Assert.Equal(42.0, number.Value);
    }

    [Fact]
    public void Parse_Decimal_ReturnsNumberLiteral()
    {
        var ast = Parse("3.14");

        var number = Assert.IsType<NumberLiteral>(ast);
        Assert.Equal(3.14, number.Value);
    }

    [Fact]
    public void Parse_SimpleAddition_ReturnsBinaryOpAdd()
    {
        var ast = Parse("2 + 3");

        var binary = Assert.IsType<BinaryOp>(ast);
        Assert.Equal(BinaryOperator.Add, binary.Operator);

        var left = Assert.IsType<NumberLiteral>(binary.Left);
        Assert.Equal(2.0, left.Value);

        var right = Assert.IsType<NumberLiteral>(binary.Right);
        Assert.Equal(3.0, right.Value);
    }

    [Fact]
    public void Parse_RightAssociativePower_BuildsRightLeaningTree()
    {
        var ast = Parse("2 ^ 3 ^ 2");

        var outer = Assert.IsType<BinaryOp>(ast);
        Assert.Equal(BinaryOperator.Power, outer.Operator);

        var leftLeaf = Assert.IsType<NumberLiteral>(outer.Left);
        Assert.Equal(2.0, leftLeaf.Value);

        var inner = Assert.IsType<BinaryOp>(outer.Right);
        Assert.Equal(BinaryOperator.Power, inner.Operator);
    }

    [Fact]
    public void Parse_Parentheses_OverridePrecedence()
    {
        var ast = Parse("(1 + 2) * 3");

        var multiply = Assert.IsType<BinaryOp>(ast);
        Assert.Equal(BinaryOperator.Multiply, multiply.Operator);

        var add = Assert.IsType<BinaryOp>(multiply.Left);
        Assert.Equal(BinaryOperator.Add, add.Operator);
    }


    [Fact]
    public void Parse_FunctionCallOneArg_ReturnsFunctionCallWithOneArgument()
    {
        var ast = Parse("sqrt(16)");

        var call = Assert.IsType<FunctionCall>(ast);
        Assert.Equal("sqrt", call.Name);
        Assert.Single(call.Arguments);

        var arg = Assert.IsType<NumberLiteral>(call.Arguments[0]);
        Assert.Equal(16.0, arg.Value);
    }

    [Fact]
    public void Parse_FunctionCallTwoArgs_ReturnsFunctionCallWithTwoArguments()
    {
        var ast = Parse("max(3, 5)");

        var call = Assert.IsType<FunctionCall>(ast);
        Assert.Equal("max", call.Name);
        Assert.Equal(2, call.Arguments.Count);
    }

}
