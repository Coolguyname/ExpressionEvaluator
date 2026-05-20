namespace ExpressionEvaluator.Core.Evaluating;

public sealed class VariablesParseException : Exception
{
    public VariablesParseException(string message) : base(message) { }
}
