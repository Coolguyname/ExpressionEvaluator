using System;
using System.Globalization;

namespace ExpressionEvaluator.Core.Evaluating;

public static class VariablesParser
{
    public static Value ParseValue(string text)
    {
        text = text.Trim();
        if (text.Equals("true", StringComparison.OrdinalIgnoreCase)) return new BooleanValue(true); 
        if (text.Equals("false", StringComparison.OrdinalIgnoreCase)) return new BooleanValue(false); 
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)) return new NumberValue(number);
        throw new VariablesParseException($"Cannot parse value '{text}'");
    }


    public static Variables Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Variables.Empty;
        }

        var dict = new Dictionary<string, Value>();
        var pairs = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                throw new VariablesParseException($"Invalid variable assignment: '{pair}'. Expected 'name = value'.");
            }

            var name = parts[0];
            var rawValue = parts[1];
            dict[name] = ParseValue(rawValue);
        }

        return new Variables(dict);
    }
}
