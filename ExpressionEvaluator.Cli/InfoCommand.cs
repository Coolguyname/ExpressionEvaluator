using ExpressionEvaluator.Core.Evaluating;

namespace ExpressionEvaluator.Cli;

public static class InfoCommand
{
    public static int Run(string[] args)
    {
        string? file = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--file" when i + 1 < args.Length:
                    file = args[++i];
                    break;
                default:
                    throw new CliException($"Unexpected argument: {args[i]}");
            }
        }

        if (file is null)
            throw new CliException("Missing --file argument.");

        var csv = CsvReader.Read(file);

        Console.WriteLine($"File: {file}");
        Console.WriteLine($"Columns: {csv.Headers.Count}");
        Console.WriteLine($"Rows: {csv.Rows.Count}\n");
        Console.WriteLine($"{"Column",-20}{"Type"}");

        for (int i = 0; i < csv.Headers.Count; i++)
        {
            var name = csv.Headers[i];
            string type = "unknown";
            if (csv.Rows.Count > 0 && i < csv.Rows[0].Length)
            {
                try
                {
                    var v = VariablesParser.ParseValue(csv.Rows[0][i]);
                    type = v is NumberValue ? "Number" : "Boolean";
                }
                catch (VariablesParseException)
                {
                    type = "Text";
                }
            }
            Console.WriteLine($"{name,-20}{type}");
        }
        return 0;
    }
}

