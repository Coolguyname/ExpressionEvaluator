using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionEvaluator.Core.Evaluating;

public sealed class Variables
{
    private readonly IReadOnlyDictionary<string, Value> values;

    public Variables(IReadOnlyDictionary<string, Value> values) { this.values = values; }

    public static Variables Empty { get; } = new Variables(new Dictionary<string, Value>());

    public Value Get(string name, int position)
    {
        if (values.TryGetValue(name, out var value)) return value;

        throw new EvaluatorException($"Unknown variable '{name}'", position);
    }
}
