using ExpressionEvaluator.Core.Evaluating;
using ExpressionEvaluator.Core.Lexing;
using ExpressionEvaluator.Core.Parsing;
using ExpressionEvaluator.Core.Parsing.Ast;

namespace ExpressionEvaluator.Cli;

public static class EvaluateCommand
{
    public static int Run(string[] args)
    {
        string? expression = null;
        string? vars = null;
        bool showTokens = false;
        bool showAst = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--vars" when i + 1 < args.Length:
                    vars = args[++i];
                    break;
                case "--show-tokens":
                    showTokens = true;
                    break;
                case "--show-ast":
                    showAst = true;
                    break;
                default:
                    if (expression is null) expression = args[i];
                    else throw new CliException($"Unexpected argument: {args[i]}");
                    break;
            }
        }

        if (expression is null)
            throw new CliException("No expression provided");

        var tokens = new Lexer(expression).Tokenize();
        if (showTokens)
        {
            Console.WriteLine($"{"Position",-10}{"Kind",-15}{"Lexeme"}");
            foreach (var token in tokens)
            {
                Console.WriteLine($"{token.Position,-10}{token.Kind,-15}{token.Lexeme}");
            }
        }

        var ast = new Parser(tokens).Parse();
        if (showAst)
        {
            PrintAst(ast, 0);
        }

        Variables variables = vars is not null
            ? VariablesParser.Parse(vars)
            : AskForVariables(ast);

        var value = new Evaluator().Evaluate(ast, variables);
        Console.WriteLine($"Result: {ValueFormatter.Format(value)}");

        return 0;
    }

    private static Variables AskForVariables(Expression ast)
    {
        var names = VariableCollector.Collect(ast);
        if (names.Count == 0) return Variables.Empty;

        var dict = new Dictionary<string, Value>();
        foreach (var name in names)
        {
            Console.Write($"{name} = ");
            var input = Console.ReadLine() ?? "";
            dict[name] = VariablesParser.ParseValue(input);
        }
        return new Variables(dict);
    }

    private static void PrintAst(Expression expression, int depth)
    {
        Console.WriteLine($"{new string(' ', depth * 2)}{Describe(expression)}");

        foreach (var child in Children(expression))
        {
            PrintAst(child, depth + 1);
        }
        Console.WriteLine();
    }

    private static string Describe(Expression expression) => expression switch
    {
        NumberLiteral n => $"Number: {n.Value}",
        BooleanLiteral b => $"Boolean: {b.Value}",
        Variable v => $"Variable: {v.Name}",
        UnaryOp u => $"UnaryOp: {u.Operator}",
        BinaryOp b => $"BinaryOp: {Evaluator.OperatorSymbol(b.Operator)}",
        FunctionCall f => $"FunctionCall: {f.Name}",
        _ => expression.GetType().Name
    };

    private static IReadOnlyList<Expression> Children(Expression expression) => expression switch
    {
        UnaryOp u => [u.Operand],
        BinaryOp b => [b.Left, b.Right],
        FunctionCall f => [.. f.Arguments],
        _ => []
    };
}
