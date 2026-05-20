namespace ExpressionEvaluator.Cli;

public sealed record CsvData(IReadOnlyList<string> Headers, IReadOnlyList<string[]> Rows);

public static class CsvReader
{
    public static CsvData Read(string path)
    {
        if (!File.Exists(path))
            throw new CliException($"File not found: {path}");

        var lines = File.ReadAllLines(path);

        if (lines.Length == 0)
            throw new CliException($"File is empty: {path}");

        var headers = lines[0].Split(';', StringSplitOptions.TrimEntries);

        var rows = new List<string[]>();
        foreach (var row in lines[1..])
        {
            if (string.IsNullOrWhiteSpace(row))
                continue;
            rows.Add(row.Split(';', StringSplitOptions.TrimEntries));
        }

        return new CsvData(headers, rows);
    }
}
