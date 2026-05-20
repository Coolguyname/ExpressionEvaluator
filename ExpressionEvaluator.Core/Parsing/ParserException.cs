namespace ExpressionEvaluator.Core.Parsing;

public sealed class ParserException : Exception
{
    public int Position { get; }

    public ParserException(string message, int position) : base(message)
    {
        Position = position;
    }
}