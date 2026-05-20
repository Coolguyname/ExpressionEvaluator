using ExpressionEvaluator.Core.Evaluating;
using ExpressionEvaluator.Core.Lexing;
using ExpressionEvaluator.Core.Parsing;

namespace ExpressionEvaluator.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help")
        {
            PrintHelp();
            return 0;
        }

        var command = args[0];
        var rest = args[1..];

        try
        {
            return command switch
            {
                "evaluate" => EvaluateCommand.Run(rest),
                "filter" => FilterCommand.Run(rest),
                "info" => InfoCommand.Run(rest),
                _ => throw new CliException($"Unrecognized command: {command}")
            };
        }
        catch (LexerException ex)
        {
            Console.Error.WriteLine($"Lexer error: {ex.Message}");
            return 1;
        }
        catch (ParserException ex)
        {
            Console.Error.WriteLine($"Parser error: {ex.Message} (position {ex.Position})");
            return 1;
        }
        catch (EvaluatorException ex)
        {
            Console.Error.WriteLine($"Evaluator error: {ex.Message} (position {ex.Position})");
            return 1;
        }
        catch (VariablesParseException ex)
        {
            Console.Error.WriteLine($"Variables error: {ex.Message}");
            return 1;
        }
        catch (CliException ex)
        {
            Console.Error.WriteLine($"CLI error: {ex.Message}");
            return 1;
        }

    }

    private static void PrintHelp()
    {
        Console.WriteLine("Expression Evaluator CLI");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  ExpressionEvaluator.Cli <command> [arguments]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine();
        Console.WriteLine("  evaluate <expression> [--vars \"x=5,y=10\"] [--show-tokens] [--show-ast]");
        Console.WriteLine("      Evaluates an expression. If the expression contains variables and");
        Console.WriteLine("      --vars is not provided, user will be asked to input them.");
        Console.WriteLine();
        Console.WriteLine("  filter --file <path> --where <expression> [--show-rows]");
        Console.WriteLine("      Reads a CSV file and counts rows where the expression is true.");
        Console.WriteLine("      Numeric and boolean columns become variables in the expression.");
        Console.WriteLine();
        Console.WriteLine("  info --file <path>");
        Console.WriteLine("      Shows column names and types of a CSV file.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --help   Show this help message.");
        Console.WriteLine();
    }
}
