using System;
using System.Globalization;

namespace ExpressionEvaluator.Core.Evaluating;

public static class ValueFormatter
{
    public static string Format(Value value) => value switch
    {
        NumberValue n => n.Number.ToString(CultureInfo.InvariantCulture),
        BooleanValue b => b.Boolean ? "true" : "false",
        _ => throw new ArgumentException($"Unable to foramt value of type: {value?.GetType().Name ?? "null"}")
    };
}
