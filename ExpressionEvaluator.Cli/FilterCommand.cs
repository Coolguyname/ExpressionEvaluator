using ExpressionEvaluator.Core.Evaluating;
using ExpressionEvaluator.Core.Lexing;
using ExpressionEvaluator.Core.Parsing;

namespace ExpressionEvaluator.Cli;

public static class FilterCommand
{
    public static int Run(string[] args)
    {
        string? file = null;
        string? where = null;
        bool showRows = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--file" when i + 1 < args.Length:
                    file = args[++i];
                    break;
                case "--where" when i + 1 < args.Length:
                    where = args[++i];
                    break;
                case "--show-rows":
                    showRows = true;
                    break;
                default:
                    throw new CliException($"Unexpected argument: {args[i]}");
            }
        }

        if (file is null) throw new CliException("Missing --file argument.");
        if (where is null) throw new CliException("Missing --where argument.");

        var csv = CsvReader.Read(file);

        var tokens = new Lexer(where).Tokenize();
        var ast = new Parser(tokens).Parse();
        var evaluator = new Evaluator();

        int matched = 0;
        foreach (var row in csv.Rows)
        {
            var variables = RowToVariables(csv.Headers, row);
            var result = evaluator.Evaluate(ast, variables);

            if (result is not BooleanValue boolean)
                throw new CliException("--where expression must evaluate to a boolean");

            if (boolean.Boolean)
            {
                matched++;
                if (showRows)
                    Console.WriteLine(string.Join(";", row));
            }
        }

        Console.WriteLine($"{matched} of {csv.Rows.Count} rows matched.");
        return 0;
    }

    private static Variables RowToVariables(IReadOnlyList<string> headers, string[] row)
    {
        var dict = new Dictionary<string, Value>();

        for (int i = 0; i < headers.Count; i++)
        {
            var meno = headers[i];

            if (i >= row.Length) continue;

            try
            {
                dict[meno] = VariablesParser.ParseValue(row[i]);
            }
            catch (VariablesParseException)
            {
                continue;
            }
        }
        return new Variables(dict);
    }
}
